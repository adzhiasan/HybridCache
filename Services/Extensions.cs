namespace Services;

public static class Extensions
{
    public static async Task Migrate(this IServiceProvider serviceProvider)
    {
        using var scoped = serviceProvider.CreateScope();
        var dbContext = scoped.ServiceProvider.GetRequiredService<AppDbContext>();
        
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        dbContext.Stores.AddRange(SeedData.Stores);
        dbContext.Products.AddRange(SeedData.Products);

        await dbContext.SaveChangesAsync();
    }
}