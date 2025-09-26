using MediatR;

namespace FindPet.BusinessLogicLayer.CQRS.Common;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}