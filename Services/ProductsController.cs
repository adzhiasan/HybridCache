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
    public Task<List<Product>?> GetProductsAsync(int storeId)
    {
        return _hybridCache.GetOrAddAsync(
            $"products-{storeId}",
            async () => await _dbContext.Products.Where(x => x.Store.Id == storeId).Include(x => x.Store).ToListAsync(),
            TimeSpan.FromSeconds(10));
    }

    [HttpGet("/stores")]
    public async Task<IEnumerable<Store>?> GetStoresAsync()
    {
        return await _hybridCache.GetOrAddAsync(
            "stores",
            async () => await _dbContext.Stores.ToListAsync(),
            TimeSpan.FromSeconds(10));
    }
}