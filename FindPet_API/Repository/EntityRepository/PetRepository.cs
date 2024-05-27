using Models.Entities;
using System.Security.Cryptography.Xml;
using Interfaces.IEntityRepository;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Repository.EntityRepository;

public class PetRepository : BaseRepository<Pet>, IPetRepository
{
    private readonly FindPetDbContext _context;

    public PetRepository(FindPetDbContext context) : base(context)
    {
        _context = context;
    }

    //public async Task<IEnumerable<Pet>> GetsAsync()
    //{
    //    return await GetAllAsync().Result.ToListAsync();
    //}

    //public async Task<Pet?> GetAsync(Guid petId)
    //{
    //    return await GetByConditionAsync(x => x.Id == petId).Result.FirstOrDefaultAsync();
    //}

    ////public async Task<IEnumerable<Ad>?> GetAdsByPet(Guid petId)
    ////{
    ////    return await GetByConditionAsync(x => x.Id == petId).Result.Include(x => x.Ads).SelectMany(x => x.Ads).ToListAsync();
    ////}

    ////public async Task<Finder> GetFinderByPet(Guid petId)
    ////{
    ////    return await GetByConditionAsync(x => x.Id == petId).Result.Include(x => x.Finder).Select(x => x.Finder).FirstOrDefaultAsync();
    ////}

    ////public async Task<Owner> GetOwnerByPet(Guid petId)
    ////{
    ////    return await GetByConditionAsync(x => x.Id == petId).Result.Include(x => x.Owner).Select(x => x.Owner).FirstOrDefaultAsync();
    ////}

    //public async Task<bool> IsExistAsync(Guid petId)
    //{
    //    return await ExistsAsync(x => x.Id == petId);
    //}

    public async Task<bool> IsExistAsync(string petName)
    {
        return await IsExistAsync(x => x.Nickname == petName);
    }

    //public async Task DeleteAsync(Guid petId)
    //{
    //    await DeleteAsync(petId);
    //}

    //public async Task UpdateAsync(Pet pet)
    //{
    //    await UpdateAsync(pet);
    //}

    //public async Task CreateAsync(Pet pet)
    //{
    //    await CreateAsync(pet);
    //}
}