using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.CQRS.Queries.Account;
using FindPet.BusinessLogicLayer.Interfaces.IAuthService;
using FindPet.Domain.DTOs.AuthDTOs;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.QueryHandlers.Account;

public class GetActiveSessionsQueryHandler(IAuthService authService)
    : IQueryHandler<GetActiveSessionsQuery, IEnumerable<SessionDto>>
{
    public async Task<IEnumerable<SessionDto>> Handle(GetActiveSessionsQuery request,
        CancellationToken cancellationToken)
    {
        return await authService.GetActiveSessionsAsync(request.UserId, request.CurrentToken);
    }
}