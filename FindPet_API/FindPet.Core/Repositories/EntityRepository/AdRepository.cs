using FindPet.Domain.Entities;
using FindPet.Infrastructure.Data;
using FindPet.Infrastructure.Interfaces.IEntityRepository;

namespace FindPet.Core.Repositories.EntityRepository;

public class AdRepository : BaseRepository<Ad>, IAdRepository
{
    private readonly FindPetDbContext _context;

    public AdRepository(FindPetDbContext context) : base(context)
    {
        _context = context;
    }

    //public new IEnumerable<Ad> GetsAsync()
    //{
    //    return base.GetAllAsync();
    //}

    //public async Task<Ad?> GetAsync(Guid adId)
    //{
    //    return await GetByConditionAsync(x => x.Id == adId).Result.FirstOrDefaultAsync();
    //}

    //public async Task<Pet?> GetPetByAd(Guid adId)
    //{
    //    return await GetByConditionAsync(x => x.Id == adId).Result.Include(x => x.Pet).Select(x => x.Pet).FirstOrDefaultAsync();
    //}

    //public async Task<User> GetUserByAd(Guid adId)
    //{
    //    return await GetByConditionAsync(x => x.Id == adId).Result.Include(x => x.User).Select(x => x.User).FirstOrDefaultAsync();
    //}

    //public async Task<bool> IsExistAsync(Guid adId)
    //{
    //    return await IsExistAsync(x => x.Id == adId);
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
}