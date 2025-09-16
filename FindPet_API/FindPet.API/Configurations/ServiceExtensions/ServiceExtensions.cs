using FindPet.Core.Services.EntityService;
using FindPet.Core.Services.MLService;
using FindPet.Infrastructure.Interfaces.IEntityService;
using FindPet.Infrastructure.Interfaces.IMLService;
using Microsoft.AspNetCore.Identity;

namespace FindPet.API.Configurations.ServiceExtensions;

public static class ServiceExtensions
{
    public static void ConfigureServices(this IServiceCollection services)
    {
        services.AddScoped<IPetService, PetService>();
        services.AddScoped<IAdService, AdService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IMLService, MLService>();
        services.AddScoped<RoleManager<IdentityRole>>();
        services.AddControllers();
    }
}