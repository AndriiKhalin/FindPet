using FindPet.Core.Services.EntityService;
using FindPet.Infrastructure.Interfaces.IEntityService;

namespace FindPet.API.Configurations.ServiceExtensions;

public static class ServiceExtensions
{
    public static void ConfigureServices(this IServiceCollection services)
    {
        services.AddScoped<IPetService, PetService>();
        services.AddScoped<IOwnerService, OwnerService>();
        services.AddScoped<IFinderService, FinderService>();
        services.AddScoped<IAdService, AdService>();
    }
}