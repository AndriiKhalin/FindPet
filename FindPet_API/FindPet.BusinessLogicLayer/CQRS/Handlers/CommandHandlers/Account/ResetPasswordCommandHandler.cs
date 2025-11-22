using FindPet.BusinessLogicLayer.CQRS.Commands.Account;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.Interfaces.IAuthService;
using FindPet.Domain.ValueObjects;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.Account;

public class ResetPasswordCommandHandler(IAuthService authService)
    : ICommandHandler<ResetPasswordCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var success = await authService.ResetPasswordAsync(
            request.ResetPassword.Email,
            request.ResetPassword.Token,
            request.ResetPassword.NewPassword);

        return new AuthResponse
        {
            IsSuccess = success,
            Message = success
                ? "Password has been reset successfully. You can now login with your new password."
                : "Password reset failed. Invalid or expired token."
        };
    }
}