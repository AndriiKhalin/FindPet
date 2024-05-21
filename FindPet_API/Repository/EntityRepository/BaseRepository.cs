using Interfaces.IEntityRepository;
using Microsoft.EntityFrameworkCore;
using Models.Entities;
using Models;

namespace Repository.EntityRepository;

public class BaseRepository<T> : HelperBaseRepository<T>, IBaseRepository<T> where T : BaseEntity
{
    private readonly FindPetDbContext _context;

    public BaseRepository(FindPetDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<T>> GetsAsync()
    {
        return await GetAllAsync().Result.ToListAsync();
    }

    public async Task<T?> GetAsync(Guid userId)
    {
        return await GetByConditionAsync(x => x.Id == userId).Result.FirstOrDefaultAsync();
    }

    public async Task<bool> IsExistAsync(Guid userId)
    {
        return await ExistsAsync(x => x.Id == userId);
    }

    public async Task DeleteAsync(Guid entityId)
    {
        await DeleteAsync(entityId);
    }

    public async Task UpdateAsync(T entity)
    {
        await UpdateAsync(entity);
    }

    public async Task CreateAsync(T entity)
    {
        await CreateAsync(entity);
    }
}