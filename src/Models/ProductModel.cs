using System.ComponentModel.DataAnnotations;

namespace DeliveryApi.Models;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public bool IsPublic { get; set; } = false;

    [Required]
    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    [Required]
    public Category Category { get; set; } = new();

    [Required]
    [Range(0, double.MaxValue)]
    public double Price { get; set; }
}
