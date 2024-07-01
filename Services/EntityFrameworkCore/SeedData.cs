using Services;

public static class SeedData
{
    public static List<Store> Stores { get; } = new();
    public static List<Product> Products { get; } = new();

    static SeedData()
    {
        var store1 = new Store
        {
            StoreOpen = TimeOnly.FromTimeSpan(TimeSpan.FromHours(9)),
            StoreClose = TimeOnly.FromTimeSpan(TimeSpan.FromHours(23)),
            Address = "asd"
        };
        var store2 = new Store
        {
            StoreOpen = TimeOnly.FromTimeSpan(TimeSpan.FromHours(7)),
            StoreClose = TimeOnly.FromTimeSpan(TimeSpan.FromHours(22)),
            Address = "asd"
        };
        
        var product1 = new Product
        {
            Price = 100,
            Store = store1,
            StoreId = store1.Id
        };
        var product2 = new Product
        {
            Price = 150,
            Store = store1,
            StoreId = store1.Id
        };
        var product3 = new Product
        {
            Price = 500,
            Store = store2,
            StoreId = store2.Id
        };
        var product4 = new Product
        {
            Price = 300,
            Store = store2,
            StoreId = store2.Id
        };
        var product5 = new Product
        {
            Price = 250,
            Store = store2,
            StoreId = store2.Id
        };
        
        Stores.AddRange([store1, store2]);
        Products.AddRange([product1, product2, product3, product4, product5]);
    }
}