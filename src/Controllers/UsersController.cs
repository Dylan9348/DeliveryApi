using System.IdentityModel.Tokens.Jwt;
using DeliveryApi.DataBase;
using DeliveryApi.Models;
using DeliveryApi.Models.RequestModels;
using DeliveryApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApi.Controllers;

[ApiController]
[Route("users")]
public class UsersController(Context database, ITokenService tokenService) : Controller
{
    private readonly Context _database = database;
    private readonly ITokenService _tokenService = tokenService;

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserReq req)
    {
        var passwordhash = BCrypt.Net.BCrypt.HashPassword(req.Password);
        var user = new UserModel
        {
            Username = req.Username,
            Password = passwordhash,
            Role = UserRole.Client,
            Points = 0,
        };

        var userAlreadyTaken = await _database.Users.AnyAsync(u => u.Username == req.Username);

        if (userAlreadyTaken)
        {
            return Conflict();
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

        var token = _tokenService.GenerateJwtToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        await _tokenService.SetRefreshTokenAsync(refreshToken, user.Username);

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

        var stored = await _database.RefreshTokens.FirstOrDefaultAsync(r =>
            r.Token == refreshToken
        );

        if (stored is null || stored.Expiration < DateTime.UtcNow)
            return Unauthorized("Invalid or expired refresh token");

        if (stored.Revoked)
            return Unauthorized("Revoked");

        var user = await _database.Users.FirstOrDefaultAsync(u =>
            u.Username == stored.UsernameTarget
        );

        if (user is null)
            return Unauthorized("Invalid");

        /*
            return Jwt and a new Refresh Token
        */

        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var newJwtToken = _tokenService.GenerateJwtToken(user);

        stored.Revoked = true;

        await _tokenService.SetRefreshTokenAsync(newRefreshToken, user.Username);

        return Ok(newJwtToken);
    }

    [HttpDelete("self")]
    [Authorize]
    public async Task<IActionResult> Delete([FromBody] string password)
    {
        var claimUserId = User.FindFirst(JwtRegisteredClaimNames.Sub);

        if (claimUserId is null)
            return Unauthorized();

        var stringUserId = claimUserId.Value;

        if (!Guid.TryParse(stringUserId, out Guid userId))
            return Unauthorized();

        var user = await _database.Users.FindAsync(userId);

        if (user is null)
            return NotFound("User not found.");

        var isPasswordCorrect = BCrypt.Net.BCrypt.Verify(password, user.Password);

        if (!isPasswordCorrect)
            return Unauthorized("Incorrect password.");

        _database.Users.Remove(user);
        await _database.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _database.Users.Select(u => u.Username).ToListAsync();
        return Ok(users);
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpPost("suspend")]
    public async Task<IActionResult> SuspendAccount(SuspendAccountRequest req)
    {
        var claimUserId = User.FindFirst(JwtRegisteredClaimNames.Sub);

        if (claimUserId is null)
            return Unauthorized();

        var stringUserId = claimUserId.Value;
        var client = await _database.Users.FirstOrDefaultAsync(u =>
            u.Id.ToString() == stringUserId
        );

        var user = await _database.Users.FirstOrDefaultAsync(u => u.Username == req.Username);

        if (user is null)
            return NotFound(req.Username);

        if (
            client is null
            || (user.Role == UserRole.Admin && client.Role != UserRole.Owner)
            || user.Role == UserRole.Owner
        )
            return Unauthorized();

        DateTime until;

        if (user.SuspendedUntil is null || user.SuspendedUntil < DateTime.UtcNow)
            until = DateTime.UtcNow;
        else
            until = (DateTime)user.SuspendedUntil;

        user.SuspendedUntil = until.AddDays(req.Days).AddHours(req.Hours);

        await _database.SaveChangesAsync();

        return Ok();
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpDelete("suspend")]
    public async Task<IActionResult> UnsuspendAccount(string username)
    {
        var user = await _database.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user is null)
            return NotFound(username);

        user.SuspendedUntil = null;

        return Ok();
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpPost("roles")]
    public async Task<IActionResult> AssignRole(AssignRoleRequest req)
    {
        var user = await _database.Users.FirstOrDefaultAsync(u => u.Username == req.Username);

        if (user is null)
            return NotFound(req.Username);

        if (!Enum.TryParse(typeof(UserRole), req.Role, out var obj) || obj is not UserRole role)
            return BadRequest();

        if (role == UserRole.Owner || (role == UserRole.Admin && User.IsInRole("Admin")))
            return Forbid();

        user.Role = role;
        await _database.SaveChangesAsync();

        return Ok();
    }

    [Authorize(Roles = "Owner")]
    [HttpDelete]
    public async Task<IActionResult> DeleteAccount([FromBody] string username)
    {
        var user = await _database.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user is null)
            return NotFound(username);

        _database.Users.Remove(user);
        await _database.SaveChangesAsync();
        return Ok();
    }
}
