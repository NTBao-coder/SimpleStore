using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleStore.Data;

public class IdentityDbContextDesignTimeFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        // Schema generation needs no server or password. For database update, supply the environment variable.
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__identitydb")
            ?? "Host=localhost;Database=identitydb;Username=postgres";
        var options = new DbContextOptionsBuilder<IdentityDbContext>().UseNpgsql(connection);
        var services = new ServiceCollection();
        services.Configure<IdentityOptions>(options =>
            options.Stores.SchemaVersion = IdentitySchemaVersions.Version3);
        options.UseApplicationServiceProvider(services.BuildServiceProvider());
        return new IdentityDbContext(options.Options);
    }
}
