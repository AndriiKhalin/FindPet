using FindPet.DataAccessLayer.Data;
using FindPet.DataAccessLayer.Interfaces.IEntityRepository;
using FindPet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FindPet.DataAccessLayer.Repositories.EntityRepository;

public class BaseRepository<T> : HelperBaseRepository<T>, IBaseRepository<T> where T : BaseEntity
{
    private readonly FindPetDbContext _context;

    public BaseRepository(FindPetDbContext context) : base(context)
    {
        _context = context;
    }

    public IEnumerable<T> Gets()
    {
        return GetAll();
    }

    public async Task<T?> GetAsync(Guid Id)
    {
        return await GetByConditionAsync(x => x.Id == Id).Result.FirstOrDefaultAsync();
    }

    public async Task<bool> IsExistAsync(Guid Id)
    {
        return await IsExistAsync(x => x.Id == Id);
    }

    public new async Task DeleteAsync(Guid entityId)
    {
        await base.DeleteAsync(entityId);
    }

    public new async Task UpdateAsync(T entity)
    {
        await base.UpdateAsync(entity);
    }

    public new async Task CreateAsync(T entity)
    {
        await base.CreateAsync(entity);
    }
}