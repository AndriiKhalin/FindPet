using FindPet.Domain.Entities;
using FindPet.Domain.ValueObjects;
using Microsoft.IdentityModel.Tokens;
using TokenValidationResult = FindPet.Domain.ValueObjects.TokenValidationResult;

namespace FindPet.BusinessLogicLayer.Interfaces.IAuthService;

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(AuthUser user);
    Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string ipAddress);
    Task<AuthResponse> RefreshTokenAsync(string token, string ipAddress);
    Task<bool> RevokeTokenAsync(string token, string ipAddress, string? reason = null);
    Task RevokeUserTokensAsync(string userId, string ipAddress);
    Task<TokenValidationResult> ValidateTokenAsync(string token);
    Task CleanupExpiredTokensAsync();
}