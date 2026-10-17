using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SimpleStore.Catalog.API.Data;

public class CatalogDbContextDesignTimeFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        // Schema generation needs no server or password. For database update, supply the environment variable.
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__catalogdb")
            ?? "Host=localhost;Database=catalogdb;Username=postgres";
        var options = new DbContextOptionsBuilder<CatalogDbContext>().UseNpgsql(connection);
        return new CatalogDbContext(options.Options);
    }
}
