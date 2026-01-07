using Microsoft.Extensions.DependencyInjection;

namespace FindPet.Infrastructure.Configurations.ServiceExtensions;

public static class CorsExtension
{
    public static void ConfigureCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder.WithOrigins(
                        "http://localhost:4200",
                        "https://localhost:4200",
                        "http://localhost:4000",
                        "https://localhost:4000"
                    )
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
        });
    }
}