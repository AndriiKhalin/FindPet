using FindPet.DataAccessLayer.Data;
using FindPet.DataAccessLayer.Interfaces.IEntityRepository;
using FindPet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FindPet.DataAccessLayer.Repositories.EntityRepository;

public class RefreshTokenRepository(FindPetDbContext context)
    : BaseRepository<RefreshToken>(context), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await context.Set<RefreshToken>()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId)
    {
        return await context.Set<RefreshToken>()
            .Where(rt => rt.UserId == userId &&
                         rt.RevokedAt == null &&
                         rt.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync();
    }

    public async Task<RefreshToken> GetActiveTokenByUserIdAsync(string userId)
    {
        return await context.Set<RefreshToken>()
            .Where(rt => rt.UserId == userId &&
                         rt.RevokedAt == null &&
                         rt.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(rt => rt.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<RefreshToken>> GetAllTokensByUserIdAsync(string userId)
    {
        return await context.Set<RefreshToken>()
            .Where(rt => rt.UserId == userId)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<RefreshToken>> GetExpiredTokensAsync()
    {
        return await context.Set<RefreshToken>()
            .Where(rt => rt.ExpiresAt < DateTime.UtcNow || rt.RevokedAt != null)
            .ToListAsync();
    }
}