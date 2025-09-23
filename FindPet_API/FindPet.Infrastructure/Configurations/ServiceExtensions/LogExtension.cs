using FindPet.BusinessLogicLayer.Interfaces.ILoggerService;
using FindPet.BusinessLogicLayer.Services.LoggerService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FindPet.Infrastructure.Configurations.ServiceExtensions;

public static class LogExtension
{
    public static void ConfigureLoggerService(this IServiceCollection services)
    {
        services.AddSingleton<ILoggerManager, LoggerManager>();
        services.AddLogging(logging => { logging.AddConsole(); });
    }
}