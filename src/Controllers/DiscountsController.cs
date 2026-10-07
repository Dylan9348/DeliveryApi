
using System.ComponentModel.DataAnnotations;
using DeliveryApi.DataBase;
using DeliveryApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApi.Controllers;

[Route("discounts")]
public class DiscountsController(IDiscountsService discountsService, Context database) : Controller
{
    private readonly IDiscountsService _discountsService = discountsService;
    private readonly Context _database = database;

    [Authorize(Roles = "Admin,Owner")]
    [HttpPost("categories/{categoryName}")]
    public async Task<IActionResult> SetCategoryDiscount(string categoryName, int discount)
    {
        if (discount < 0 || discount > 100)
            return BadRequest("The discount can't be greater than 100 or less than 0.");

        var category = await _database.Categories.FirstOrDefaultAsync(c => c.Name == categoryName);

        if (category is null)
            return NotFound(categoryName);

        category.Discount = discount;

        await _database.Products.Where(p => p.Category.Id == category.Id)
            .ForEachAsync((p) =>
            {
                p.Category = category;
            });

        await _database.SaveChangesAsync();

        return Ok();
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpPost("products/{productName}")]
    public async Task<IActionResult> SetProductDiscount(string productName, int discount)
    {
        if (discount < 0 || discount > 100)
            return BadRequest("The discount can't be grater than 100 or less than 0.");
        
        var product = await _database.Products.FirstOrDefaultAsync(p => p.Name == productName);

        if (product is null)
            return NotFound(productName);
    
        product.Discount = discount;

        await _database.SaveChangesAsync();

        return Ok();
    }
}
