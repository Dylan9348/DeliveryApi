
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DeliveryApi.DataBase;
using DeliveryApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DeliveryApi.Controllers;

[ApiController]
[Route("users")]
public class UsersController(Context database) : Controller
{
    private readonly Context _database = database;
    
    private static string GenerateJwtToken(UserModel user)
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

    private string GenerateRefreshToken()
    {
        var bytes = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private async Task SetRefreshTokenCookie(string refreshToken, string username)
    {
        var token = new RefreshToken()
        {
            Token = refreshToken,
            UsernameTarget = username
        };

        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(15)
        });

        _database.RefreshTokens.Add(token);
        await _database.SaveChangesAsync();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserReq req)
    {
        var pw_hash = BCrypt.Net.BCrypt.HashPassword(req.Password);
        var user = new UserModel
        {
            Username    = req.Username,
            Password    = pw_hash,
            Role        = UserRole.Client,
            Points      = 0
        };

        var userAlreadyTaken = await _database.Users.AnyAsync(u => u.Username == req.Username);

        if (userAlreadyTaken)
        {
            return Conflict(new Dictionary<string, string>()
                {
                    {"body", "username already taken"},
                    {"field", "username"}
                });
        }

        _database.Users.Add(user);
        await _database.SaveChangesAsync();

        return Created();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserReq req)
    {
        var user = await _database.Users.FirstOrDefaultAsync(u => u.Username == req.Username);

        if (user is null)
            return NotFound();
        
        var isPasswordCorrect = BCrypt.Net.BCrypt.Verify(req.Password, user.Password);

        if (!isPasswordCorrect)
            return Unauthorized();

        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        await SetRefreshTokenCookie(refreshToken, user.Username);

        return Ok(token);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        /*
            validate refresh token
        */

        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            return Unauthorized("No refresh token");

        var stored = await _database.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken);

        if (stored is null || stored.Expiration < DateTime.UtcNow)
            return Unauthorized("Invalid or expired refresh token");

        if (stored.Revoked)
            return Unauthorized("Revoked");

        var user = await _database.Users.FirstOrDefaultAsync(u => u.Username == stored.UsernameTarget);

        if (user is null)
            return Unauthorized("Invalid");

        /*
            return Jwt and a new Refresh Token
        */

        var newRefreshToken = GenerateRefreshToken();
        var newJwtToken = GenerateJwtToken(user);

        stored.Revoked = true;

        await SetRefreshTokenCookie(newRefreshToken, user.Username);

        return Ok(newJwtToken);
    }

    [HttpGet]
    public async Task<IActionResult> ListUsers()
    {
        var titles = await _database.Users
            .Select(u => u.Username)
            .ToListAsync();
        
        return Ok(titles);
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteUser([FromBody] string password)
    {
        var claimUserId = User.FindFirst(JwtRegisteredClaimNames.Sub);

        if (claimUserId is null)
            return Unauthorized();
        
        var stringUserId = claimUserId.Value;

        if (!Guid.TryParse(stringUserId, out Guid userId))
            return Ok();
        
        var user = await _database.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            return NotFound("User not found.");

        var isPasswordCorrect = BCrypt.Net.BCrypt.Verify(password, user.Password);

        if (!isPasswordCorrect)
            return Unauthorized();

        _database.Users.Remove(user);
        await _database.SaveChangesAsync();

        return NoContent();
    }
}
