using System.Diagnostics;
using FindPet.Domain.Interfaces.ILoggerService;
using MediatR;

namespace FindPet.BusinessLogicLayer.CQRS.Behaviors;

public class PerformanceBehavior<TRequest, TResponse>(ILoggerManager logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly Stopwatch _timer = new();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500) // Log if request takes longer than 500ms
        {
            var requestName = typeof(TRequest).Name;

            logger.LogWarn(
                $"FindPet Long Running Request: {requestName} ({elapsedMilliseconds} milliseconds) {request}");
        }

        return response;
    }
}