using FindPet.BusinessLogicLayer.Interfaces.IAuthService;
using FindPet.Domain.Interfaces.ILoggerService;

namespace FindPet.WebApi.BackgroundServices;

public class TokenCleanupService(IServiceProvider serviceProvider, ILoggerManager logger) : BackgroundService
{
    private readonly TimeSpan _period = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInfo("Token Cleanup Service started");
        using var timer = new PeriodicTimer(_period);

        while (!stoppingToken.IsCancellationRequested &&
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = serviceProvider.CreateScope();
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            await tokenService.CleanupExpiredTokensAsync();

            logger.LogInfo("Token cleanup completed successfully");
        }
    }
}