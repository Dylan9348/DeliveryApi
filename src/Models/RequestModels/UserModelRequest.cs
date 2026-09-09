using System.ComponentModel.DataAnnotations;

namespace DeliveryApi.Models.RequestModels;

public class UserReq
{
    [Required]
    [StringLength(int.MaxValue, MinimumLength = 4)]
    public string Username { get; set; } = "";

    [Required]
    [StringLength(int.MaxValue, MinimumLength = 8)]
    public string Password { get; set; } = "";
}
