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
        var products = _hybridCache.GetOrAdd(
            $"products-{storeId}",
            () => _dbContext.Products.Where(x => x.Store.Id == storeId).Include(x => x.Store).ToList(),
            TimeSpan.FromSeconds(10));
        
        return products;
    }
    
    [HttpGet("/stores")]
    public IEnumerable<Store>? GetStores()
    {
        var stores = _hybridCache.GetOrAdd(
            "stores",
            () => _dbContext.Stores.ToList(),
            TimeSpan.FromSeconds(10));
        
        return stores;
    }
}