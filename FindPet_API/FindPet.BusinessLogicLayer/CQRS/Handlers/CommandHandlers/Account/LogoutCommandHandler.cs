using FindPet.BusinessLogicLayer.CQRS.Commands.Account;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.Interfaces.IAuthService;
using FindPet.Domain.Interfaces.ILoggerService;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.Account;

public class LogoutCommandHandler(ITokenService tokenService, ILoggerManager logger)
    : ICommandHandler<LogoutCommand, bool>
{
    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            logger.LogWarn("Logout attempted with empty refresh token");
            return true;
        }

        await tokenService.RevokeTokenAsync(request.RefreshToken, "User logout");

        return true;
    }
}