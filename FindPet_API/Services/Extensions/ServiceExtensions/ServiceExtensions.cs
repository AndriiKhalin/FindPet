using Interfaces.IEntityService;

using Microsoft.Extensions.DependencyInjection;
using Repository;
using Services.Service.EntityService;

namespace Services.Extensions.ServiceExtensions;

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