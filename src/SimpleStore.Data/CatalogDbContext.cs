using Microsoft.EntityFrameworkCore;
using SimpleStore.Data.Models;

namespace SimpleStore.Data;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);
    }
}
