
using DeliveryApi.Models;
using DeliveryApi.DataBase;

using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace DeliveryApi.Services;

public class TokenService(Context database, IHttpContextAccessor httpContextAccessor) : ITokenService
{
    private readonly Context _database = database;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public string GenerateJwtToken(UserModel user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("username", user.Username),
            new Claim("role", user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var secretKey = Environment.GetEnvironmentVariable("KEY_TOKEN_GEN") ?? throw new InvalidOperationException("KEY_TOKEN_GEN no está configurada");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "delivery-api",
            audience: "clients",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    public async Task SetRefreshTokenAsync(string refreshToken, string username)
    {
        var token = new RefreshToken()
        {
            Token = refreshToken,
            UsernameTarget = username
        };

        _httpContextAccessor.HttpContext?.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(15)
        });

        _database.RefreshTokens.Add(token);
        await _database.SaveChangesAsync();
    }
}
