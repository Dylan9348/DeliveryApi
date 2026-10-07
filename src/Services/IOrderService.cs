using DeliveryApi.Models;
using DeliveryApi.Models.DtoModels;

namespace DeliveryApi.Services;

public interface IOrderService
{
    Task RegisterOrder(UserDto client, Product[] products, Address? address, string code);
    Task<double> QuoteAllPricesUsingPoints(Guid[] productsId, int points);
    Task AddPoints(Guid clientId, Guid[] productsId);
}
