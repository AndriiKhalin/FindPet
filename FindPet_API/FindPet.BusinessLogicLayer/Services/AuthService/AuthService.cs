using FindPet.BusinessLogicLayer.Helpers.UrlHelper;
using FindPet.BusinessLogicLayer.Interfaces.IAuthService;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.Domain.DTOs.AuthDTOs;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;
using FindPet.Domain.Entities;
using FindPet.Domain.Exceptions;
using FindPet.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

        // Email confirmation
        var emailToken = await userManager.GenerateEmailConfirmationTokenAsync(authUser);

        // TODO: Send confirmation email with token
        // await _emailService.SendConfirmationEmailAsync(authUser.Email, emailToken);

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
            Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
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

    public async Task<UserDetailDto> GetCurrentUserAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
            throw new NotFoundException("User", userId);

        var roles = await userManager.GetRolesAsync(user);

        return new UserDetailDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Photo = user.Photo,
            BirthDate = user.BirthDate,
            Role = roles.ToArray(),
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            AccessFailedCount = user.AccessFailedCount,
            TwoFactorEnabled = user.TwoFactorEnabled
        };
    }

    public async Task<IEnumerable<UserDetailDto>> GetAllUsersAsync()
    {
        var users = await userManager.Users.ToListAsync();
        var userDtos = new List<UserDetailDto>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            userDtos.Add(new UserDetailDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Role = roles.ToArray(),
                PhoneNumber = user.PhoneNumber,
                BirthDate = user.BirthDate,
                Photo = user.Photo,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                AccessFailedCount = user.AccessFailedCount,
                TwoFactorEnabled = user.TwoFactorEnabled
            });
        }

        return userDtos;
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
            throw new NotFoundException("User", email);

        return await userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
            return false;

        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded;
    }

    private async Task EnsureRolesExistAsync()
    {
        if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
            await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));

        if (!await roleManager.RoleExistsAsync(UserRoles.User))
            await roleManager.CreateAsync(new IdentityRole(UserRoles.User));
    }
}