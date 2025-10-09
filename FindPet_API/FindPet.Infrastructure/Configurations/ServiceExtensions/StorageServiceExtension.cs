using FindPet.Domain.ValueObjects;
using FindPet.Media.Interfaces;
using FindPet.Media.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FindPet.Infrastructure.Configurations.ServiceExtensions;

public static class StorageServiceExtension
{
    public static void ConfigureStorageService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureBlobStorageOptions>(
            configuration.GetSection(AzureBlobStorageOptions.SectionName));

        services.AddScoped<IMediaStorageService>(provider =>
        {
            var connectionString = configuration["AzureStorage:ConnectionString"];
            var containerName = configuration["AzureStorage:ContainerName"];
            var options = provider.GetRequiredService<IOptions<AzureBlobStorageOptions>>();

            return new AzureBlobStorageService(connectionString, options, containerName);
        });
    }
}