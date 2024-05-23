using Interfaces.IEntityRepository;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Entities;

namespace Repository.EntityRepository;

public class OwnerRepository : BaseRepository<Owner>, IUserRepository<Owner>
{
    public OwnerRepository(FindPetDbContext context) : base(context)
    {
    }


    //public async Task<IEnumerable<Owner>> GetsAsync()
    //{
    //    return await GetAllAsync().Result.ToListAsync();
    //}

    //public async Task<Owner?> GetAsync(Guid userId)
    //{
    //    return await GetByConditionAsync(x => x.Id == userId).Result.FirstOrDefaultAsync();
    //}

    ////public async Task<IEnumerable<Ad>?> GetAdsByOwner(Guid ownerId)
    ////{
    ////    return await GetByConditionAsync(x => x.Id == ownerId).Result.Include(x => x.Ads).SelectMany(x => x.Ads).ToListAsync();
    ////}

    ////public async Task<IEnumerable<Pet>?> GetPetsByOwner(Guid ownerId)
    ////{
    ////    return await GetByConditionAsync(x => x.Id == ownerId).Result.Include(x => x.Pets).SelectMany(x => x.Pets).ToListAsync();
    ////}

    //public async Task<bool> IsExistAsync(Guid userId)
    //{
    //    return await ExistsAsync(x => x.Id == userId);
    //}

    public async Task<bool> IsExistAsync(string userFirstName)
    {
        return await IsExistAsync(x => x.FirstName == userFirstName);
    }

    //public async Task DeleteAsync(Guid userId)
    //{
    //    await DeleteAsync(userId);
    //}

    //public async Task UpdateAsync(User user)
    //{
    //    await UpdateAsync(user);
    //}

    //public async Task CreateAsync(User user)
    //{
    //    await CreateAsync(user);
    //}

}