using FindPet.Domain.DTOs.AuthDTOs;
using FindPet.Domain.ValueObjects;

namespace FindPet.BusinessLogicLayer.Interfaces.IAuthService;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);

    Task<UserDetailDto> GetCurrentUserAsync(string userId);

    Task<IEnumerable<UserDetailDto>> GetAllUsersAsync();

    Task<string> GeneratePasswordResetTokenAsync(string email);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
}