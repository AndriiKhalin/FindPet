using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.CQRS.Queries.Account;
using FindPet.BusinessLogicLayer.Interfaces.IAuthService;
using FindPet.Domain.DTOs.AuthDTOs;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.QueryHandlers.Account;

public class GetAllAuthUsersQueryHandler(IAuthService authService)
    : IQueryHandler<GetAllAuthUsersQuery, IEnumerable<UserDetailDto>>
{
    public async Task<IEnumerable<UserDetailDto>> Handle(GetAllAuthUsersQuery request,
        CancellationToken cancellationToken)
    {
        return await authService.GetAllUsersAsync();
    }
}