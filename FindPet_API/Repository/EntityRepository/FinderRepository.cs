using Interfaces.IEntityRepository;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Entities;

namespace Repository.EntityRepository;

public class FinderRepository : BaseRepository<Finder>, IFinderRepository
{
    private readonly FindPetDbContext _context;

    public FinderRepository(FindPetDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Finder>> GetFindersAsync()
    {
        return await GetAllAsync().Result.ToListAsync();
    }

    public async Task<Finder?> GetFinderAsync(Guid finderId)
    {
        return await GetByConditionAsync(x => x.Id == finderId).Result.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Ad>?> GetAdsByFinder(Guid finderId)
    {
        return await GetByConditionAsync(x => x.Id == finderId).Result.Include(x => x.Ads).SelectMany(x => x.Ads).ToListAsync();
    }

    public async Task<IEnumerable<Pet>?> GetPetsByFinder(Guid finderId)
    {
        return await GetByConditionAsync(x => x.Id == finderId).Result.Include(x => x.Pets).SelectMany(x => x.Pets).ToListAsync();
    }

    public async Task<bool> FinderExistsAsync(Guid finderId)
    {
        return await ExistsAsync(x => x.Id == finderId);
    }

    public async Task<bool> FinderExistsAsync(string finderFirstName)
    {
        return await ExistsAsync(x => x.FirstName == finderFirstName);
    }

    public async Task DeleteFinderAsync(Guid finderId)
    {
        await DeleteAsync(finderId);
    }

    public async Task UpdateFinderAsync(Finder finder)
    {
        await UpdateAsync(finder);
    }

    public async Task CreateFinderAsync(Finder finder)
    {
        await CreateAsync(finder);
    }
}