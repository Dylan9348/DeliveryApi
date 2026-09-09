using DeliveryApi.DataBase;
using DeliveryApi.Models;
using DeliveryApi.Models.DtoModels;
using DeliveryApi.Models.RequestModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApi.Controllers;

[ApiController]
[Route("products")]
public class ProductsController(Context database) : Controller
{
    private readonly Context _database = database;

    [HttpGet]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] string? category = null,
        [FromQuery] string? name = null,
        [FromQuery] double? maxPrice = null,
        [FromQuery] double? minPrice = null
    )
    {
        category = string.IsNullOrWhiteSpace(category) ? null : category;
        name = string.IsNullOrWhiteSpace(name) ? null : name;

        if (minPrice > maxPrice)
            return BadRequest();

        var products = await _database
            .Products.Where(p =>
                p.IsPublic
                && (category == p.Category.Name || category == null)
                && (name == null || EF.Functions.ILike(p.Name, $"%{name}%"))
                && (maxPrice == null || maxPrice >= p.Price)
                && (minPrice == null || minPrice <= p.Price)
            )
            .Select(p => new ProductDto
            {
                Name = p.Name,
                Description = p.Description,
                Category = p.Category.Name,
                Price = p.Price,
            })
            .ToListAsync();

        return Ok(products);
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllProducts()
    {
        var products = await _database.Products.ToListAsync();

        return Ok(products);
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpPost]
    public async Task<IActionResult> CreateProduct(ProductReq productReq)
    {
        var exists = await _database.Products.AnyAsync(p => p.Name == productReq.Name);

        if (exists)
            return Conflict(productReq.Name);

        var category = await _database.Categories.FirstOrDefaultAsync(c =>
            c.Name == productReq.Category
        );

        if (category is null)
            return BadRequest("Invalid category");

        var product = new Product()
        {
            Name = productReq.Name,
            Description = productReq.Description,
            Category = category,
            Price = productReq.Price,
        };

        _database.Products.Add(product);

        await _database.SaveChangesAsync();

        return Created();
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpPut("{name}")]
    public async Task<IActionResult> EditProduct(string name, ProductReq newProduct)
    {
        var oldProduct = await _database.Products.FirstOrDefaultAsync(p => p.Name == name);

        if (oldProduct is null)
            return NotFound(name);

        var exists = await _database.Products.AnyAsync(p => p.Name == newProduct.Name);

        if (exists)
            return Conflict(newProduct.Name);

        var category = await _database.Categories.FirstOrDefaultAsync(c =>
            c.Name == newProduct.Category
        );

        if (category is null)
            return BadRequest("Invalid category");

        oldProduct.Name = newProduct.Name;
        oldProduct.Description = newProduct.Description;
        oldProduct.Category = category;
        oldProduct.Price = newProduct.Price;

        return Ok();
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpDelete("{name}")]
    public async Task<IActionResult> DeleteProduct(string name)
    {
        var product = await _database.Products.FirstOrDefaultAsync(p => p.Name == name);

        if (product is null)
            return NotFound(name);

        _database.Products.Remove(product);
        await _database.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpPost("{productName}/publish")]
    public async Task<IActionResult> PublishProduct(string productName)
    {
        var product = await _database.Products.FirstOrDefaultAsync(p => p.Name == productName);

        if (product is null)
            return NotFound(productName);

        product.IsPublic = true;

        await _database.SaveChangesAsync();

        return Ok();
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpPost("{productName}/unpublish")]
    public async Task<IActionResult> UnpublishProduct(string productName)
    {
        var product = await _database.Products.FirstOrDefaultAsync(p => p.Name == productName);

        if (product is null)
            return NotFound(productName);

        product.IsPublic = false;

        await _database.SaveChangesAsync();

        return Ok();
    }
}
