using Interfaces.IEntityRepository;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Entities;

namespace Repository.EntityRepository;

public class OwnerRepository : BaseRepository<Owner>, IOwnerRepository
{
    public OwnerRepository(FindPetDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Owner>> GetOwnersAsync()
    {
        return await GetAllAsync().Result.ToListAsync();
    }

    public async Task<Owner?> GetOwnerAsync(Guid ownerId)
    {
        return await GetByConditionAsync(x => x.Id == ownerId).Result.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Ad>?> GetAdsByOwner(Guid ownerId)
    {
        return await GetByConditionAsync(x => x.Id == ownerId).Result.Include(x => x.Ads).SelectMany(x => x.Ads).ToListAsync();
    }

    public async Task<IEnumerable<Pet>?> GetPetsByOwner(Guid ownerId)
    {
        return await GetByConditionAsync(x => x.Id == ownerId).Result.Include(x => x.Pets).SelectMany(x => x.Pets).ToListAsync();
    }

    public async Task<bool> OwnerExistsAsync(Guid ownerId)
    {
        return await ExistsAsync(x => x.Id == ownerId);
    }

    public async Task<bool> OwnerExistsAsync(string ownerFirstName)
    {
        return await ExistsAsync(x => x.FirstName == ownerFirstName);
    }

    public async Task DeleteOwnerAsync(Guid ownerId)
    {
        await DeleteAsync(ownerId);
    }

    public async Task UpdateOwnerAsync(Owner owner)
    {
        await UpdateAsync(owner);
    }

    public async Task CreateOwnerAsync(Owner owner)
    {
        await CreateAsync(owner);
    }
}