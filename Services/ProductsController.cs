using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Services;

public class ProductsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ProductsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<IEnumerable<Product>> GetProducts(int storeId)
    {
        var products = await _dbContext.Products.Where(x => x.Store.Id == storeId).ToListAsync();
        return products;
    }
}

public class Product
{
    public int Id { get; init; }
    public int Price { get; init; }
    public Store Store { get; init; }
    public int StoreId { get; set; }
}

public class Store
{
    public int Id { get; init; }
    public TimeOnly StoreOpen { get; init; }
    public TimeOnly StoreClose { get; init; }
    public string Address { get; init; }
}
public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Store> Stores { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : base(dbContextOptions)
    {
        
    }
}