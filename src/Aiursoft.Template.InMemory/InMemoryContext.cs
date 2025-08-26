using Aiursoft.Template.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Template.InMemory;

public class InMemoryContext(DbContextOptions<InMemoryContext> options) : FlyClassDbContext(options)
{
    public override Task MigrateAsync(CancellationToken cancellationToken)
    {
        return Database.EnsureCreatedAsync(cancellationToken);
    }

    public override Task<bool> CanConnectAsync()
    {
        return Task.FromResult(true);
    }
}
