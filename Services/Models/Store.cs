namespace Services;

public class Store
{
    public int Id { get; init; }
    public TimeOnly StoreOpen { get; init; }
    public TimeOnly StoreClose { get; init; }
    public string Address { get; init; } = null!;
}