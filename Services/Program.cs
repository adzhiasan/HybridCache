using HybridCache;
using Microsoft.EntityFrameworkCore;
using Services;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.AddControllers();
services.AddDbContext<AppDbContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

services.AddSelfWrittenHybridCache(
    options =>
    {
        options.Configuration = "localhost";
        options.InstanceName = "local";
    });

var app = builder.Build();

var dbContext = app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

await dbContext.Database.EnsureDeletedAsync();
await dbContext.Database.EnsureCreatedAsync();

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
dbContext.Stores.AddRange(store1, store2);
dbContext.Products.AddRange(product1, product2, product3, product4, product5);
await dbContext.SaveChangesAsync();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

app.MapControllers();

app.Run();