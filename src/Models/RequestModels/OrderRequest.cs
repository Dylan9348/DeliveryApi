using System.ComponentModel.DataAnnotations;

namespace DeliveryApi.Models.RequestModels;

public class OrderRequest
{
    [Required]
    public string[] Products { get; set; } = [];
    
    public Address? ClientAddress { get; set; } = null;
}
