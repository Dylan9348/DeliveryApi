
using DeliveryApi.DataBase;

namespace DeliveryApi.Services;

public class DiscountsService(Context database) : IDiscountsService
{
    private readonly Context _database = database;

    public async Task<double> CalcMaxPointDiscountAsync(Guid[] products)
    {
        var productsPrice = await CalcProductsDiscountAsync(products);

        return CalcMaxPointDiscount(productsPrice);
    }

    public double CalcMaxPointDiscount(double productsPrice)
    {
        return productsPrice * 50;
    }

    public async Task<double> CalcPointsDiscountAsync(int points, Guid[] products)
    {
        var productsPrice = await CalcProductsDiscountAsync(products);

        return CalcPointsDiscount(points, productsPrice);
    }

    public double CalcPointsDiscount(int points, double productsPrice)
    {
        var maxPointDiscount = CalcMaxPointDiscount(productsPrice);

        if (maxPointDiscount < points)
            return -1;
        
        var discount = points / (productsPrice * 100) * productsPrice;
        
        return productsPrice - discount;
    }

    public async Task<double> CalcProductsDiscountAsync(Guid[] products)
    {
        var finalPrice = 0.0;

        foreach (var p in products)
            finalPrice += await CalcDiscountAsync(p);
        
        return finalPrice;
    }

    public async Task<double> CalcDiscountAsync(Guid productId)
    {
        var product = await _database.Products.FindAsync(productId);

        if (product is null)
            return -1;
        
        var finalPrice = product.Price 
            - (product.Discount / 100 * product.Price) 
            - (product.Category.Discount / 100 * product.Price);

        return finalPrice;
    }
}
