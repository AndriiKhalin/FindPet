using FindPet.Domain.DTOs.AuthDTOs;
using FindPet.Domain.ValueObjects;

namespace FindPet.BusinessLogicLayer.Interfaces.IAuthService;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);

    Task<UserDetailDto> GetCurrentUserAsync(string userId);

    Task<IEnumerable<UserDetailDto>> GetAllUsersAsync();

    // Password Management
    Task<string> GeneratePasswordResetTokenAsync(string email);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword);

    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);

    // Email Confirmation

    Task<AuthResponse> ConfirmEmailAsync(string userId, string token);
    Task<string> GenerateEmailConfirmationTokenAsync(string userId);

    // Session Management

    Task<IEnumerable<SessionDto>> GetActiveSessionsAsync(string userId, string currentToken);
    Task<bool> RevokeSessionAsync(string userId, Guid tokenId);
}