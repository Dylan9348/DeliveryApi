
using System.ComponentModel.DataAnnotations;

namespace DeliveryApi.Models;

public class ProductReq
{
    [Required]
    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    [Required]
    public string Category { get; set; } = "";

    [Required]
    [Range(0, double.MaxValue)]
    public double Price { get; set; }

    public Product ToProduct()
    {
        return new Product
        {
            Name = Name,
            Description = Description,
            Category = Category,
            Price = Price
        };
    }
}
