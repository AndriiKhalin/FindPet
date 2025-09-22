using FindPet.DataAccessLayer.Data;
using FindPet.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FindPet.Infrastructure.Configurations.AuthExtensions;

public static class IdentityConfigurationExtension
{
    public static void AddIdentityConfiguration(this IServiceCollection services)
    {
        services.AddIdentity<AuthUser, IdentityRole>(options =>
            {
                // Configure Identity options here
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<FindPetDbContext>()
            .AddDefaultTokenProviders();
    }
}