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
            .ToListAsync();
    }

    public async Task<IEnumerable<RefreshToken>> GetExpiredTokensAsync()
    {
        return await context.Set<RefreshToken>()
            .Where(rt => rt.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();
    }
}