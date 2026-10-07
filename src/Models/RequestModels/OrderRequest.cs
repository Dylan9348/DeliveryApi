using System.ComponentModel.DataAnnotations;

namespace DeliveryApi.Models.RequestModels;

public class OrderRequest
{
    [Required]
    public string[] Products { get; set; } = [];
    
    [Range(0, int.MaxValue)]
    public int? Points { get; set; }

    public Address? ClientAddress { get; set; } = null;
}
