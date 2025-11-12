using FindPet.BusinessLogicLayer.CQRS.Commands.Account;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.Interfaces.IAuthService;
using FindPet.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.Account;

public class LoginCommandHandler : ICommandHandler<LoginCommand, AuthResponse>
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly UserManager<AuthUser> _userManager;

    public LoginCommandHandler(
        UserManager<AuthUser> userManager,
        IConfiguration configuration,
        IAuthService authService)
    {
        _userManager = userManager;
        _configuration = configuration;
        _authService = authService;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(request.LoginDto, cancellationToken);
    }
}