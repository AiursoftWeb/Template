using Aiursoft.Scanner.Abstractions;
using Aiursoft.Template.Configuration;
using Aiursoft.Template.Entities;
using Aiursoft.Template.Models;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Template.Services;

public class GlobalSettingsService(TemplateDbContext dbContext, IConfiguration configuration) : IScopedDependency
{
    public async Task<string> GetSettingValueAsync(string key)
    {
        // 1. Check environment variable
        var envValue = configuration[key];
        if (!string.IsNullOrWhiteSpace(envValue))
        {
            return envValue;
        }

        // 2. Check database
        var dbSetting = await dbContext.GlobalSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == key);
        if (dbSetting != null && dbSetting.Value != null)
        {
            return dbSetting.Value;
        }

        // 3. Fallback to default
        var definition = SettingsMap.Definitions.FirstOrDefault(d => d.Key == key);
        return definition?.DefaultValue ?? string.Empty;
    }

    public async Task<bool> GetBoolSettingAsync(string key)
    {
        var value = await GetSettingValueAsync(key);
        return bool.TryParse(value, out var result) && result;
    }

    public bool IsOverriddenByEnv(string key)
    {
        return !string.IsNullOrWhiteSpace(configuration[key]);
    }

    public async Task UpdateSettingAsync(string key, string value)
    {
        if (IsOverriddenByEnv(key))
        {
            throw new InvalidOperationException($"Setting {key} is overridden by environment variable and cannot be updated in database.");
        }

        var definition = SettingsMap.Definitions.FirstOrDefault(d => d.Key == key) 
                         ?? throw new InvalidOperationException($"Setting {key} is not defined.");

        // Validation
        switch (definition.Type)
        {
            case SettingType.Bool:
                if (!bool.TryParse(value, out _))
                {
                    throw new InvalidOperationException($"Value '{value}' is not a valid boolean for setting {key}.");
                }
                break;
            case SettingType.Number:
                if (!double.TryParse(value, out _))
                {
                    throw new InvalidOperationException($"Value '{value}' is not a valid number for setting {key}.");
                }
                break;
            case SettingType.Choice:
                if (definition.ChoiceOptions != null && !definition.ChoiceOptions.ContainsKey(value))
                {
                    throw new InvalidOperationException($"Value '{value}' is not a valid choice for setting {key}.");
                }
                break;
            case SettingType.Text:
            default:
                break;
        }

        var dbSetting = await dbContext.GlobalSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (dbSetting == null)
        {
            dbSetting = new GlobalSetting { Key = key, Value = value };
            dbContext.GlobalSettings.Add(dbSetting);
        }
        else
        {
            dbSetting.Value = value;
        }

        await dbContext.SaveChangesAsync();
    }
    
    public async Task SeedSettingsAsync()
    {
        foreach (var definition in SettingsMap.Definitions)
        {
            var exists = await dbContext.GlobalSettings.AnyAsync(s => s.Key == definition.Key);
            if (!exists)
            {
                dbContext.GlobalSettings.Add(new GlobalSetting
                {
                    Key = definition.Key,
                    Value = definition.DefaultValue
                });
            }
        }
        await dbContext.SaveChangesAsync();
    }
}
