
using Microsoft.EntityFrameworkCore;

using DeliveryApi.DataBase;

namespace DeliveryApi.Extensions;

public static class DataBaseExtension
{
    public static IServiceCollection AddDataBase(this IServiceCollection services)
    {
        services.AddDbContext<Context>((options) =>
        {
            options.UseNpgsql(Environment.GetEnvironmentVariable("NPGSQL_CONNECTION"));
        });
        
        return services;
    }
}