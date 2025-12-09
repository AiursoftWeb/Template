using Aiursoft.Template.Entities;
using Microsoft.EntityFrameworkCore;


namespace Aiursoft.Template.Sqlite;

public class SqliteContext(DbContextOptions<SqliteContext> options) : TemplateDbContext(options)
{
    public override Task<bool> CanConnectAsync()
    {
        return Task.FromResult(true);
    }
}
