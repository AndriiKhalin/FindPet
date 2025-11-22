using FindPet.BusinessLogicLayer.CQRS.Commands.Account;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.Interfaces.IAuthService;
using FindPet.Domain.ValueObjects;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.Account;

public class ForgotPasswordCommandHandler(IAuthService authService)
    : ICommandHandler<ForgotPasswordCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        await authService.GeneratePasswordResetTokenAsync(request.ForgotPassword.Email);

        return new AuthResponse
        {
            IsSuccess = true,
            Message = "Password reset instructions have been sent to your email"
        };
    }
}