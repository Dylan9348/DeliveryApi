using DeliveryApi.Services;

namespace DeliveryApi.Extensions;

public static class ServicesExtension
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPointsService, PointsService>();
        services.AddScoped<IDiscountsService, DiscountsService>();
        
        return services;
    }
}
