
using System.ComponentModel.DataAnnotations;

namespace DeliveryApi.Models;

public class UserModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(int.MaxValue, MinimumLength= 4)]
    public string Username { get; set; } = "";

    [Required]
    [StringLength(int.MaxValue, MinimumLength= 8)]
    public string Password { get; set; } = "";

    [Required]
    public UserRole Role { get; set; }
    
    public int Points { get; set; } = 0;
}
