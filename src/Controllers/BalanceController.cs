using System.IdentityModel.Tokens.Jwt;
using DeliveryApi.DataBase;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryApi.Controllers;

[Route("balance")]
public class BalanceController(Context database) : Controller
{
    private readonly Context _database = database;

    [HttpGet]
    public async Task<IActionResult> GetPointsCount()
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

        return Ok(user.Points);
    }
}