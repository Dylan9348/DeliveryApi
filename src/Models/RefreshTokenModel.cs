
namespace DeliveryApi.Models;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Token { get; set; } = "";
    public string UsernameTarget { get; set; } = "";
    public DateTime Expiration { get; set; } = DateTime.UtcNow.AddDays(15);
    public bool Revoked { get; set; } = false;
}
