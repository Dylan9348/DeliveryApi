namespace DeliveryApi.Models;

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public int ProductsCount { get; set; } = 0;
}
