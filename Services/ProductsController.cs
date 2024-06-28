using HybridCache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Services;

public class ProductsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IHybridCache _hybridCache;

    public ProductsController(AppDbContext dbContext, IHybridCache hybridCache)
    {
        _dbContext = dbContext;
        _hybridCache = hybridCache;
    }
    
    [HttpGet("/products")]
    public IEnumerable<Product>? GetProducts(int storeId)
    {
        var products = _hybridCache.GetOrAdd($"products-{storeId}", () =>
        {
            return _dbContext.Products.Where(x => x.Store.Id == storeId).Include(x => x.Store).ToList();
        }, TimeSpan.FromSeconds(10));
        
        return products;
    }
    
    [HttpGet("/stores")]
    public IEnumerable<Store>? GetProducts()
    {
        var stores = _hybridCache.GetOrAdd($"stores", () =>
        {
            return _dbContext.Stores.ToList();
        }, TimeSpan.FromSeconds(10));
        
        return stores;
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