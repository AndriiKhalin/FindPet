using FindPet.Core.Services.LoggerService;
using FindPet.Infrastructure.Interfaces.ILoggerService;

namespace FindPet.API.Configurations.ServiceExtensions;

public static class LogExtension
{
    public static void ConfigureLoggerService(this IServiceCollection services)
    {
        services.AddSingleton<ILoggerManager, LoggerManager>();
    }
}