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

    //public async Task<IEnumerable<Ad>> GetsAsync()
    //{
    //    return await GetAllAsync().Result.ToListAsync();
    //}

    //public async Task<Ad?> GetAsync(Guid adId)
    //{
    //    return await GetByConditionAsync(x => x.Id == adId).Result.FirstOrDefaultAsync();
    //}

    ////public async Task<Pet?> GetPetByAd(Guid adId)
    ////{
    ////    return await GetByConditionAsync(x => x.Id == adId).Result.Include(x => x.Pet).Select(x => x.Pet).FirstOrDefaultAsync();
    ////}

    ////public async Task<User> GetUserByAd(Guid adId)
    ////{
    ////    return await GetByConditionAsync(x => x.Id == adId).Result.Include(x => x.User).Select(x => x.User).FirstOrDefaultAsync();
    ////}

    //public async Task<bool> IsExistAsync(Guid adId)
    //{
    //    return await ExistsAsync(x => x.Id == adId);
    //}

    //public async Task DeleteAsync(Guid adId)
    //{
    //    await DeleteAsync(adId);
    //}

    //public async Task UpdateAsync(Ad ad)
    //{
    //    await UpdateAsync(ad);
    //}

    //public async Task CreateAsync(Ad ad)
    //{
    //    await CreateAsync(ad);

    //}
    public async Task<IEnumerable<Pet>> GetsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<Pet?> GetAsync(Guid entityId)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateAsync(Pet entity)
    {
        throw new NotImplementedException();
    }

    public async Task CreateAsync(Pet entity)
    {
        throw new NotImplementedException();
    }
}