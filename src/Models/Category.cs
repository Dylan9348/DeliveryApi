using System.ComponentModel.DataAnnotations;

namespace DeliveryApi.Models;

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    
    [Range(0, int.MaxValue)]
    public int ProductsCount { get; set; } = 0;

    [Range(0, 100)]
    public double Discount { get; set; } = 0;
}
