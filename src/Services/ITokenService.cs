using DeliveryApi.Models;

namespace DeliveryApi.Services;

public interface ITokenService
{
    string GenerateJwtToken(UserModel user);
    string GenerateRefreshToken();
    Task SetRefreshTokenAsync(string refreshToken, string username);
}
