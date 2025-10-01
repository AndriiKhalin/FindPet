using MediatR;

namespace FindPet.BusinessLogicLayer.CQRS.Common;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

public interface ICommand : IRequest
{
}