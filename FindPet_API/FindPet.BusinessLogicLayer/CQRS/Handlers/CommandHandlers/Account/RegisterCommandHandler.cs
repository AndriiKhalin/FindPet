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
        //var registerDto = request.RegisterDto;

        //if (!string.IsNullOrEmpty(registerDto.Photo))
        //    registerDto.Photo = PathNormalizer.NormalizePath(registerDto.Photo);

        //var id = Guid.NewGuid();

        //var user = new AuthUser
        //{
        //    Id = id.ToString(),
        //    Email = registerDto.Email,
        //    Name = registerDto.Name,
        //    UserName = registerDto.Email,
        //    PhoneNumber = registerDto.PhoneNumber,
        //    BirthDate = registerDto.BirthDate,
        //    Photo = registerDto.Photo
        //};

        //var result = await _userManager.CreateAsync(user, registerDto.Password);

        //if (!result.Succeeded)
        //    throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

        //// Ensure roles exist
        //await EnsureRolesExistAsync();

        //// Assign role
        //var roleToAssign = string.IsNullOrEmpty(registerDto.Role) ? UserRoles.User : registerDto.Role;
        //await _userManager.AddToRoleAsync(user, roleToAssign);

        //// Create user in database
        //await _userService.CreateUserAsync(new UserForCreateDto
        //{
        //    Name = registerDto.Name,
        //    Email = registerDto.Email,
        //    Password = registerDto.Password,
        //    PhoneNumber = registerDto.PhoneNumber,
        //    BirthDate = registerDto.BirthDate,
        //    Photo = registerDto.Photo
        //});

        //return new AuthResponse
        //{
        //    IsSuccess = true,
        //    Message = "Account created successfully!"
        //};
    }

    //private async Task EnsureRolesExistAsync()
    //{
    //    if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
    //        await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));

    //    if (!await _roleManager.RoleExistsAsync(UserRoles.User))
    //        await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));
    //}
}