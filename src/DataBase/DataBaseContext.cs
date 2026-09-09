using DeliveryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApi.DataBase;

public class Context(DbContextOptions<Context> context) : DbContext(context)
{
    public DbSet<UserModel> Users => Set<UserModel>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>()
            .ComplexProperty(o => o.Client);
            
        modelBuilder.Entity<Order>()
            .ComplexProperty(o => o.Delivery, d => d.IsRequired(false));

    }
}
