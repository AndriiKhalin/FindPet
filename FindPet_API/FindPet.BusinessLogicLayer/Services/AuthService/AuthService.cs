using FindPet.BusinessLogicLayer.Helpers.UrlHelper;
using FindPet.BusinessLogicLayer.Interfaces.IAuthService;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.Domain.DTOs.AuthDTOs;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;
using FindPet.Domain.Entities;
using FindPet.Domain.Exceptions;
using FindPet.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace FindPet.BusinessLogicLayer.Services.AuthService;

public class AuthService(
    UserManager<AuthUser> userManager,
    IUserService userService,
    RoleManager<IdentityRole> roleManager,
    ITokenService tokenService,
    JwtSettings jwtSettings) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterDto registerDto,
        CancellationToken cancellationToken = default)
    {
        // Validate input
        if (await userManager.FindByEmailAsync(registerDto.Email) != null)
            throw new BadRequestException("User with this email already exists");

        // Normalize photo path
        if (!string.IsNullOrEmpty(registerDto.Photo))
            registerDto.Photo = PathNormalizer.NormalizePath(registerDto.Photo);

        var userId = Guid.NewGuid().ToString();

        // Create Identity user
        var authUser = new AuthUser
        {
            Id = userId,
            Email = registerDto.Email,
            Name = registerDto.Name,
            UserName = registerDto.Email,
            PhoneNumber = registerDto.PhoneNumber,
            BirthDate = registerDto.BirthDate,
            Photo = registerDto.Photo
        };

        var result = await userManager.CreateAsync(authUser, registerDto.Password);

        if (!result.Succeeded)
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

        // Ensure roles exist
        await EnsureRolesExistAsync();

        // Assign role
        var roleToAssign = string.IsNullOrEmpty(registerDto.Role) ? UserRoles.User : registerDto.Role;
        await userManager.AddToRoleAsync(authUser, roleToAssign);

        // Create user in main database
        await userService.CreateUserAsync(new UserForCreateDto
        {
            Name = registerDto.Name,
            Email = registerDto.Email,
            Password = registerDto.Password,
            PhoneNumber = registerDto.PhoneNumber,
            BirthDate = registerDto.BirthDate,
            Photo = registerDto.Photo
        });

        return new AuthResponse
        {
            IsSuccess = true,
            Message = "Account created successfully! Please log in."
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(loginDto.Email);

        if (user is null)
            throw new UnauthorizedException("Invalid email or password");

        var isPasswordValid = await userManager.CheckPasswordAsync(user, loginDto.Password);

        if (!isPasswordValid)
            throw new UnauthorizedException("Invalid email or password");

        // Generate tokens
        var accessToken = await tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = await tokenService.GenerateRefreshTokenAsync(user.Id);

        var roles = await userManager.GetRolesAsync(user);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiration = DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes),
            RefreshTokenExpiration = refreshToken.ExpiresAt,
            IsSuccess = true,
            Message = "Login successful"
        };
    }

    public async Task<UserDetailDto> GetCurrentUserAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    private async Task EnsureRolesExistAsync()
    {
        if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
            await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));

        if (!await roleManager.RoleExistsAsync(UserRoles.User))
            await roleManager.CreateAsync(new IdentityRole(UserRoles.User));
    }
}