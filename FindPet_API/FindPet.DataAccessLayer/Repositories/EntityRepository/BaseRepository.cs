using System.Linq.Expressions;
using FindPet.DataAccessLayer.Data;
using FindPet.DataAccessLayer.Interfaces.IEntityRepository;
using FindPet.Domain.Entities;

namespace FindPet.DataAccessLayer.Repositories.EntityRepository;

public class BaseRepository<T> : HelperBaseRepository<T>, IBaseRepository<T> where T : BaseEntity
{
    private readonly FindPetDbContext _context;

    public BaseRepository(FindPetDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<T>> GetsAsync()
    {
        return await GetAllAsync();
    }

    public async Task<T?> GetAsync(Guid Id)
    {
        return await GetSingleByConditionAsync(x => x.Id == Id);
    }

    public async Task<bool> IsExistAsync(Guid Id)
    {
        return await base.IsExistAsync(x => x.Id == Id);
    }

    public new async Task<bool> IsExistAsync(Expression<Func<T, bool>> expression)
    {
        return await base.IsExistAsync(expression);
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