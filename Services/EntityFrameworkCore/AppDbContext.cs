using Microsoft.EntityFrameworkCore;

namespace Services;

public class AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : DbContext(dbContextOptions)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Store> Stores { get; set; }
}