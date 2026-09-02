using DeliveryApi.Models;
using DeliveryApi.Models.DtoModels;

namespace DeliveryApi.Services;

public interface IOrderService
{
    Task RegisterOrder(UserDto client, Product[] products, Address? address, string code);
    Task<double> QuotePrice(Product product);
    Task<double> QuoteAllPrices(Product[] products);
    //Task AddTokens(Order order);
}
