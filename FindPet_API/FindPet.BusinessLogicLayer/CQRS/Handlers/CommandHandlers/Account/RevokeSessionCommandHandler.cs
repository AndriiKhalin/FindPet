using FindPet.BusinessLogicLayer.CQRS.Commands.Account;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.Interfaces.IAuthService;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.Account;

public class RevokeSessionCommandHandler(IAuthService authService)
    : ICommandHandler<RevokeSessionCommand, bool>
{
    public async Task<bool> Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
    {
        return await authService.RevokeSessionAsync(request.UserId, request.TokenId);
    }
}