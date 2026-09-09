using DeliveryApi.DataBase;
using DeliveryApi.Models;
using DeliveryApi.Models.RequestModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApi.Controllers;

[Route("categories")]
[ApiController]
public class CategoriesController(Context database) : Controller
{
    private readonly Context _database = database;

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> ListCategories()
    {
        var categories = await _database.Categories.Select(c => c.Name).ToListAsync();
        return Ok(categories);
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpPost]
    public async Task<IActionResult> CreateCategory(CategoryReq req)
    {
        var exists = await _database.Categories.AnyAsync(c => c.Name == req.Name);

        if (exists)
            return Conflict(req.Name);

        var category = new Category() { Name = req.Name };

        _database.Categories.Add(category);
        await _database.SaveChangesAsync();

        return Created();
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpPut("{name}")]
    public async Task<IActionResult> EditCategory(string name, CategoryReq req)
    {
        var exists = await _database.Categories.AnyAsync(c => c.Name == req.Name);

        if (exists)
            return Conflict(req.Name);

        var category = await _database.Categories.FirstOrDefaultAsync(c => c.Name == name);

        if (category is null)
            return NotFound(name);

        category.Name = req.Name;

        await _database
            .Products.Where(p => p.Name == category.Name)
            .ForEachAsync(
                (p) =>
                {
                    p.Category = category;
                }
            );

        return Ok();
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpDelete("{name}")]
    public async Task<IActionResult> DeleteCategory(string name)
    {
        var category = await _database.Categories.FirstOrDefaultAsync(c => c.Name == name);

        if (category is null)
            return NotFound(name);

        if (category.ProductsCount > 0)
            return UnprocessableEntity();

        _database.Categories.Remove(category);
        await _database.SaveChangesAsync();

        return NoContent();
    }
}
