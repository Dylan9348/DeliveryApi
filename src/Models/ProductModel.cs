
using System.ComponentModel.DataAnnotations;

namespace DeliveryApi.Models;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    [Required]
    public string Category { get; set; } = "";

    [Required]
    [Range(0, double.MaxValue)]
    public double Price { get; set; }
}
