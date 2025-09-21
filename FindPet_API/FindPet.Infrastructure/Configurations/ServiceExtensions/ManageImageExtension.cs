using FindPet.BusinessLogicLayer.Interfaces.IImageService;
using FindPet.BusinessLogicLayer.Services.ImageService;
using Microsoft.Extensions.DependencyInjection;

namespace FindPet.Infrastructure.Configurations.ServiceExtensions;

public static class ManageImageExtension
{
    public static void ConfigureManageImage(this IServiceCollection services)
    {
        services.AddScoped(typeof(IManageImage<>), typeof(ManageImage<>));
    }
}