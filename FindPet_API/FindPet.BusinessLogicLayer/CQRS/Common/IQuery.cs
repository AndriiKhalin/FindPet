using MediatR;

namespace FindPet.BusinessLogicLayer.CQRS.Common;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}