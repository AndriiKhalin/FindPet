using FindPet.BusinessLogicLayer;
using FindPet.BusinessLogicLayer.Interfaces.IImageService;
using FindPet.BusinessLogicLayer.Interfaces.ILoggerService;
using FindPet.BusinessLogicLayer.Services.ImageService;
using FindPet.BusinessLogicLayer.Services.LoggerService;
using FindPet.DataAccessLayer;
using FindPet.Infrastructure.Configurations.AuthExtensions;
using FindPet.Infrastructure.Configurations.ServiceExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FindPet.Infrastructure;

public static class InfrastructureDI
{
    public static void AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add layer-specific services
        services.AddLayersServices(configuration);

        // Infrastructure-specific services
        services.ConfigureCors();
        services.Configure_FileProvider();
        services.ConfigureForm();
        services.ConfigureIISIntegration();
        services.ConfigureLoggerService();
        services.AddIdentityConfiguration();
        services.AddJwtAuthentication(configuration);
    }

    private static void AddLayersServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDataAccessServices(configuration);
        services.AddBusinessLogicServices();
    }
}