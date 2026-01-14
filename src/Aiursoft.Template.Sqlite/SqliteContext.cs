using System.Diagnostics.CodeAnalysis;
using Aiursoft.Template.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Template.Sqlite;

[ExcludeFromCodeCoverage]

public class SqliteContext(DbContextOptions<SqliteContext> options) : TemplateDbContext(options)
{
    public override Task<bool> CanConnectAsync()
    {
        return Task.FromResult(true);
    }
}
