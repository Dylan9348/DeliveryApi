using DeliveryApi.Services;

namespace DeliveryApi.Extensions;

public static class ServicesExtension
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}
