using FindPet.BusinessLogicLayer.CQRS.Commands.Account;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.Interfaces.IAuthService;
using FindPet.Domain.ValueObjects;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.Account;

public class ChangePasswordCommandHandler(IAuthService authService)
    : ICommandHandler<ChangePasswordCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var success = await authService.ChangePasswordAsync(
            request.UserId,
            request.ChangePassword.CurrentPassword,
            request.ChangePassword.NewPassword);

        return new AuthResponse
        {
            IsSuccess = success,
            Message = success
                ? "Password changed successfully"
                : "Failed to change password. Current password is incorrect."
        };
    }
}