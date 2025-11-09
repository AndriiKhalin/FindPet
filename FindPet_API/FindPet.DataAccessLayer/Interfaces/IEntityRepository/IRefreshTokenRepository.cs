using FindPet.Domain.Entities;

namespace FindPet.DataAccessLayer.Interfaces.IEntityRepository;

public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId);
    Task<IEnumerable<RefreshToken>> GetExpiredTokensAsync();
}