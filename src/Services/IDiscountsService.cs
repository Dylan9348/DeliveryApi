
namespace DeliveryApi.Services;

public interface IDiscountsService
{
    Task<double> CalcPointsDiscountAsync(int points, Guid[] products);

    Task<double> CalcProductsDiscountAsync(Guid[] products);

    Task<double> CalcDiscountAsync(Guid product);

    Task<double> CalcMaxPointDiscountAsync(Guid[] products);
    
    double CalcMaxPointDiscount(double productsPrice);

    double CalcPointsDiscount(int points, double productsPrice);
}
