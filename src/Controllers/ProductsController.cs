
using DeliveryApi.DataBase;
using DeliveryApi.Models;

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
        
        var product = productReq.ToProduct();

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
        
        oldProduct.Name = newProduct.Name;
        oldProduct.Description = newProduct.Description;
        oldProduct.Category = newProduct.Category;
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
}
