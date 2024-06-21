using FindPet.Core.Services.EntityService;
using FindPet.Infrastructure.Interfaces.IEntityService;
using Microsoft.AspNetCore.Identity;

namespace FindPet.API.Configurations.ServiceExtensions;

public static class ServiceExtensions
{
    public static void ConfigureServices(this IServiceCollection services)
    {
        services.AddScoped<IPetService, PetService>();
        services.AddScoped<IAdService, AdService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<RoleManager<IdentityRole>>();
        services.AddControllers();
    }
}