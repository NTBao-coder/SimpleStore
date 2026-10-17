using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleStore.Data;

public class OrderDbContextDesignTimeFactory : IDesignTimeDbContextFactory<OrderDbContext>
{
    public OrderDbContext CreateDbContext(string[] args)
    {
        // Schema generation needs no server or password. For database update, supply the environment variable.
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__orderdb")
            ?? "Host=localhost;Database=orderdb;Username=postgres";
        var options = new DbContextOptionsBuilder<OrderDbContext>().UseNpgsql(connection);
        return new OrderDbContext(options.Options);
    }
}
