using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.CQRS.Queries.Account;
using FindPet.BusinessLogicLayer.Interfaces.IAuthService;
using FindPet.Domain.DTOs.AuthDTOs;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.QueryHandlers.Account;

public class GetUserDetailQueryHandler(IAuthService authService) : IQueryHandler<GetUserDetailQuery, UserDetailDto>
{
    public async Task<UserDetailDto> Handle(GetUserDetailQuery request, CancellationToken cancellationToken)
    {
        return await authService.GetCurrentUserAsync(request.UserId);
    }
}