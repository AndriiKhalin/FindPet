using FindPet.Domain.Entities;
using FindPet.Domain.ValueObjects;

namespace FindPet.BusinessLogicLayer.Interfaces.IAuthService;

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(AuthUser user);
    Task<RefreshToken> GenerateRefreshTokenAsync(string userId);
    Task<AuthResponse> RefreshTokenAsync(string token);
    Task RevokeTokenAsync(string token, string? reason = null);
    Task RevokeUserTokensAsync(string userId);
    Task CleanupExpiredTokensAsync();
}