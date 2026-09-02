using System.ComponentModel.DataAnnotations;

namespace DeliveryApi.Models.RequestModels;

public class AssignRoleRequest
{
    [Required]
    public string Username { get; set; } = "";

    [Required]
    public string Role { get; set; } = "";
}
