using Interfaces.IEntityRepository;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Entities;

namespace Repository.EntityRepository;

public class AdRepository : BaseRepository<Ad>, IAdRepository
{
    public AdRepository(FindPetDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Ad>> GetAdsAsync()
    {
        return await GetAllAsync().Result.ToListAsync();
    }

    public async Task<Ad?> GetAdAsync(Guid adId)
    {
        return await GetByConditionAsync(x => x.Id == adId).Result.FirstOrDefaultAsync();
    }

    public async Task<Pet?> GetPetByAd(Guid adId)
    {
        return await GetByConditionAsync(x => x.Id == adId).Result.Include(x => x.Pet).Select(x => x.Pet).FirstOrDefaultAsync();
    }

    public async Task<IUser> GetUserByAd(Guid adId)
    {
        return await GetByConditionAsync(x => x.Id == adId).Result.Include(x => x.User).Select(x => x.User).FirstOrDefaultAsync();
    }

    public async Task<bool> AdExistsAsync(Guid adId)
    {
        return await ExistsAsync(x => x.Id == adId);
    }

    public async Task DeleteAdAsync(Guid adId)
    {
        await DeleteAsync(adId);
    }

    public async Task UpdateAdAsync(Ad ad)
    {
        await UpdateAsync(ad);
    }

    public async Task CreateAdAsync(Ad ad)
    {
        await CreateAsync(ad);

    }
}