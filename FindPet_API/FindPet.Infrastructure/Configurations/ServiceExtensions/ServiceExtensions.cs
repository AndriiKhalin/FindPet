using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.BusinessLogicLayer.Interfaces.IMLService;
using FindPet.BusinessLogicLayer.Services.EntityService;
using FindPet.BusinessLogicLayer.Services.MLService;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FindPet.Infrastructure.Configurations.ServiceExtensions;

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