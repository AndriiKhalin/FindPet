using Interfaces.ILoggerService;
using Microsoft.Extensions.DependencyInjection;
using Services.Service.LoggerService;

namespace Services.Extensions.ServiceExtensions;

public static class LogExtension
{
    public static void ConfigureLoggerService(this IServiceCollection services)
    {
        services.AddSingleton<ILoggerManager, LoggerManager>();
    }
}