using FindPet.BusinessLogicLayer.CQRS.Commands.Account;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.Interfaces.IAuthService;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.Account;

public class RegisterCommandHandler : ICommandHandler<RegisterCommand, AuthResponse>
{
    private readonly IAuthService _authService;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<AuthUser> _userManager;
    private readonly IUserService _userService;

    public RegisterCommandHandler(
        UserManager<AuthUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IUserService userService,
        IAuthService authService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _userService = userService;
        _authService = authService;
        _authService = authService;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return await _authService.RegisterAsync(request.RegisterDto, cancellationToken);
    }
}