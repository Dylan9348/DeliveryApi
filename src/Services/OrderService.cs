using DeliveryApi.DataBase;
using DeliveryApi.Models;
using DeliveryApi.Models.DtoModels;

namespace DeliveryApi.Services;

public class OrderService(Context database, IDiscountsService discountsService, IPointsService pointsService) : IOrderService
{
    private readonly Context _database = database;
    private readonly IDiscountsService _discountsService = discountsService;
    private readonly IPointsService _pointsService = pointsService;

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
    
    public async Task<double> QuoteAllPricesUsingPoints(Guid[] productsId, int points)
    {
        var priceWithDiscount = await _discountsService.CalcProductsDiscountAsync(productsId);

        var finalPrice = _discountsService.CalcPointsDiscount(points, priceWithDiscount);

        return finalPrice;
    }

    public async Task AddPoints(Guid clientId, Guid[] productsId)
    {
        var orderPrice = await _discountsService.CalcProductsDiscountAsync(productsId);

        await _pointsService.AddPointsBalanceAsync((int) orderPrice * 5, clientId);
    }
}
