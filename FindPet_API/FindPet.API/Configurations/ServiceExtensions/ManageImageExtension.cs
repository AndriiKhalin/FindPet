using FindPet.Core.Services.ImageService;
using FindPet.Infrastructure.Interfaces.IImageService;

namespace FindPet.API.Configurations.ServiceExtensions;

public static class ManageImageExtension
{
    public static void ConfigureManageImage(this IServiceCollection services)
    {
        services.AddScoped(typeof(IManageImage<>), typeof(ManageImage<>));
    }
}