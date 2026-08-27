
using DeliveryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApi.DataBase;

public class Context(DbContextOptions<Context> context) : DbContext(context)
{
    public DbSet<UserModel> Users => Set<UserModel>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Product> Products => Set<Product>();
}
