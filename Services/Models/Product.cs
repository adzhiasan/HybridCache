namespace Services;

public class Product
{
    public int Id { get; init; }
    public int Price { get; init; }
    public Store Store { get; init; } = null!;
    public int StoreId { get; set; }
}