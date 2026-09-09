using DeliveryApi.DataBase;
using DeliveryApi.Models;
using DeliveryApi.Models.DtoModels;

namespace DeliveryApi.Services;

public class OrderService(Context database) : IOrderService
{
    private readonly Context _database = database;

    public async Task RegisterOrder(
        UserDto client,
        Product[] productsName,
        Address? address,
        string code
    )
    {
        var productsId = productsName.Select(p => p.Id);

        var isAtHome = address is null;

        var order = new Order
        {
            Client = client,
            ProductsId = [.. productsId],
            IsAtHome = isAtHome,
            ClientAddress = address,
            Code = code,
        };

        _database.Add(order);
        await _database.SaveChangesAsync();
    }

    public async Task<double> QuotePrice(Product product)
    {
        return product.Price;
    }

    public async Task<double> QuoteAllPrices(Product[] products)
    {
        var finalPrice = 0.0;
        foreach (var product in products)
            finalPrice += await QuotePrice(product);
        return finalPrice;
    }

    /*
    public async Task AddPoints(Order order)
    {
        var client = await _database.Users.FindAsync(order.Client.Id);
        var delivery = await _database.Users.FindAsync(order.Delivery!.Id);
    }
    */
}
