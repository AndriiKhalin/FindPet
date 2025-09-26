using MediatR;
using System.Diagnostics;
using FindPet.BusinessLogicLayer.Interfaces.ILoggerService;

namespace FindPet.BusinessLogicLayer.CQRS.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILoggerManager logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        logger.LogInfo("Starting request {RequestName}", requestName);

        try
        {
            var response = await next(cancellationToken);

            stopwatch.Stop();
            logger.LogInfo($"Completed request {requestName} in {stopwatch.ElapsedMilliseconds}ms");

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError($"Request {requestName} failed after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}");
            throw;
        }
    }
}