using FindPet.BusinessLogicLayer;
using FindPet.DataAccessLayer;
using FindPet.Infrastructure.Configurations.AuthExtensions;
using FindPet.Infrastructure.Configurations.ServiceExtensions;
using FindPet.Media;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        services.ConfigureStorageService(configuration);
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