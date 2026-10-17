using Microsoft.EntityFrameworkCore;
using SimpleStore.Data.Models;

namespace SimpleStore.Data;

public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Order>().Property(o => o.TotalAmount).HasPrecision(18, 2);
        builder.Entity<OrderItem>().Property(i => i.UnitPrice).HasPrecision(18, 2);
    }
}
