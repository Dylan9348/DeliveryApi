using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using DeliveryApi.DataBase;
using DeliveryApi.Models.DtoModels;
using DeliveryApi.Models.RequestModels;
using DeliveryApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeliveryApi.Models;
namespace DeliveryApi.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController(Context database, IOrderService orderService) : Controller
{
    private readonly Context _database = database;
    private readonly IOrderService _orderService = orderService;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> MakeOrder(OrderRequest req)
    {
        var claimClientId = User.FindFirst(JwtRegisteredClaimNames.Sub);

        if (claimClientId is null || !Guid.TryParse(claimClientId.Value, out var clientId))
            return Unauthorized();

        var products = await _database
            .Products.Where(p => p.IsPublic && req.Products.Contains(p.Name))
            .ToArrayAsync();

        foreach (var product in req.Products)
        {
            var productsName = products.Select(p => p.Name);

            if (!productsName.Contains(product))
                return UnprocessableEntity();
        }

        var client = await _database.Users.FindAsync(clientId);

        if (client is null)
            return Unauthorized();

        var clientDto = new UserDto { UserId = client.Id, Username = client.Username };
        var code = RandomNumberGenerator.GetString("0123456789", 6);

        await _orderService.RegisterOrder(clientDto, products, req.ClientAddress, code);
        var totalCost = await _orderService.QuoteAllPrices(products);

        return Ok(new object[] { totalCost, code });
    }

    [HttpGet]
    [Authorize(Roles = "Delivery,Admin,Owner")]
    public async Task<IActionResult> ListOrders()
    {
        var orders = await _database
            .Orders.Where(o => o.IsAtHome && !o.HasDelivery)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                Address = o.ClientAddress,
                Client = o.Client,
            })
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<IActionResult> ListAllOrders()
    {
        var orders = await _database.Orders.ToListAsync();

        return Ok(orders);
    }

    [HttpPost("take")]
    [Authorize(Roles = "Delivery")]
    public async Task<IActionResult> TakeOrder(string stringOrderId)
    {
        if (!Guid.TryParse(stringOrderId, out var orderId))
            return UnprocessableEntity();

        var order = await _database.Orders.FindAsync(orderId);

        if (order is null)
            return NotFound(orderId);

        if (order.ClientAddress is null || order.HasDelivery)
            return UnprocessableEntity();

        var deliveryIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub);

        if (deliveryIdClaim is null || Guid.TryParse(deliveryIdClaim.Value, out var deliveryId))
            return Unauthorized();

        var delivery = await _database.Users.FindAsync(deliveryId);

        if (delivery is null)
            return Unauthorized();

        order.Delivery = new UserDto { UserId = delivery.Id, Username = delivery.Username };
        order.HasDelivery = true;

        await _database.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("finish/delivery")]
    [Authorize(Roles = "Delivery")]
    public async Task<IActionResult> FinishDeliveryOrder(Guid orderId, string code)
    {
        var order = await _database.Orders.FindAsync(orderId);

        if (order is null)
            return NotFound();

        if (order.Delivery is null)
            return UnprocessableEntity();

        var deliveryIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub);

        if (deliveryIdClaim is null || Guid.TryParse(deliveryIdClaim.Value, out var deliveryId))
            return Unauthorized();

        if (order.Delivery.UserId != deliveryId)
            return Forbid();

        if (order.Code != code)
            return UnprocessableEntity("Invalid code");

        order.Finished = true;

        await _database.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearOrders()
    {
        var orders = await _database.Orders.ToListAsync();

        foreach (var order in orders)
        {
            _database.Remove(order);
        }

        await _database.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("admin")]
    public async Task<IActionResult> CreateUsers()
    {
        var admin = new UserModel {
            Username = "admin",
            Role = UserRole.Admin,
            Password = BCrypt.Net.BCrypt.HashPassword("admin123")
        };

        var owner = new UserModel {
            Username = "owner",
            Role = UserRole.Owner,
            Password = BCrypt.Net.BCrypt.HashPassword("owner123")
        };

        var delivery = new UserModel {
            Username = "delivery",
            Role = UserRole.Delivery,
            Password = BCrypt.Net.BCrypt.HashPassword("delivery123")
        };

        var client = new UserModel
        {
            Username = "client",
            Role = UserRole.Client,
            Password = BCrypt.Net.BCrypt.HashPassword("client123")
        };

        _database.Users.Add(owner);
        _database.Users.Add(admin);
        _database.Users.Add(delivery);
        _database.Users.Add(client);

        await _database.SaveChangesAsync();

        return Ok();
    }
}
