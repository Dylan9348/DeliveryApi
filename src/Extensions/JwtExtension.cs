
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Text;

using DeliveryApi.DataBase;

namespace DeliveryApi.Extensions;

public static class JwtExtension
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = "delivery-api",
                    ValidAudience = "clients",
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("KEY_TOKEN_GEN")!)
                    )
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {

                        var userIdClaim = context.Principal?.FindFirst("sub")?.Value;
                        
                        if (userIdClaim is null)
                        {
                            context.Fail("Missing user id claim.");
                            return;
                        }

                        var database = context.HttpContext.RequestServices
                            .GetRequiredService<Context>();
                        
                        if (!Guid.TryParse(userIdClaim, out var userId))
                        {
                            context.Fail("Invalid user id claim");
                            return;
                        }

                        var user = await database.Users.FirstOrDefaultAsync(u => u.Id == userId);

                        if (user is null)
                        {
                            context.Fail("User no longer exists");
                            return;
                        }

                        if (user.SuspendedUntil is not null)
                        {
                            if (user.SuspendedUntil > DateTime.UtcNow)
                            {
                                context.Fail("User is suspended");
                                return;
                            }

                            user.SuspendedUntil = null;
                            await database.SaveChangesAsync();
                        }
                    }
                };
            });

        return services;
    }
} 
