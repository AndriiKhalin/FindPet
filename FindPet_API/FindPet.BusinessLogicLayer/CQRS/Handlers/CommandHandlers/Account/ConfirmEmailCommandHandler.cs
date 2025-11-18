using FindPet.BusinessLogicLayer.CQRS.Commands.Account;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.Interfaces.IAuthService;
using FindPet.Domain.ValueObjects;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.Account;

public class ConfirmEmailCommandHandler(IAuthService authService) : ICommandHandler<ConfirmEmailCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        return await authService.ConfirmEmailAsync(request.UserId, request.Token);
    }
}