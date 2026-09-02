using System.ComponentModel.DataAnnotations;

namespace DeliveryApi.Models.RequestModels;

public class CategoryReq
{
    [Required]
    public string Name { get; set; } = "";
}
