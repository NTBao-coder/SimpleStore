using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleStore.Identity.API.Data;

public sealed class IdentityDbContextDesignTimeFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__identitydb")
            ?? "Host=localhost;Database=identitydb;Username=postgres";
        var options = new DbContextOptionsBuilder<IdentityDbContext>().UseNpgsql(connection);
        var services = new ServiceCollection();
        services.Configure<IdentityOptions>(identity => identity.Stores.SchemaVersion = IdentitySchemaVersions.Version3);
        options.UseApplicationServiceProvider(services.BuildServiceProvider());
        return new IdentityDbContext(options.Options);
    }
}
