using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.BusinessLogicLayer.Interfaces.IImageService;
using FindPet.BusinessLogicLayer.Interfaces.IMLService;
using FindPet.BusinessLogicLayer.Mappings;
using FindPet.BusinessLogicLayer.Services.EntityService;
using FindPet.BusinessLogicLayer.Services.ImageService;
using FindPet.BusinessLogicLayer.Services.MLService;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FindPet_API;

public static class BusinessLogicLayerDI
{
    public static void AddBusinessLogicServices(this IServiceCollection services)
    {
        // Entity Services
        services.AddScoped<IPetService, PetService>();
        services.AddScoped<IAdService, AdService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped(typeof(IManageImage<>), typeof(ManageImage<>));
        //services.AddScoped<IFinderService, FinderService>();
        //services.AddScoped<IOwnerService, OwnerService>();

        // ML Services
        services.AddScoped<IMLService, MLService>();

        // AutoMapper
        services.AddAutoMapper(typeof(Mapping));

        //services.AddScoped<RoleManager<IdentityRole>>();
    }
}