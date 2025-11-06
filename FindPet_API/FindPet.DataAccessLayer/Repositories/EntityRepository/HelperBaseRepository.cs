using System.Linq.Expressions;
using FindPet.DataAccessLayer.Data;
using FindPet.DataAccessLayer.Interfaces.IEntityRepository;
using Microsoft.EntityFrameworkCore;

namespace FindPet.DataAccessLayer.Repositories.EntityRepository;

public abstract class HelperBaseRepository<T> : IHelperBaseRepository<T> where T : class
{
    private readonly FindPetDbContext _context;

    public HelperBaseRepository(FindPetDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var result = await _context.Set<T>().FindAsync(id);
        _context.Set<T>().Remove(result);
    }

    public IQueryable<T> GetByCondition(Expression<Func<T, bool>> expression)
    {
        return _context.Set<T>().Where(expression).AsNoTracking();
    }

    public async Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> expression)
    {
        return await _context.Set<T>().Where(expression).AsNoTracking().ToListAsync();
    }

    public async Task<T?> GetSingleByConditionAsync(Expression<Func<T, bool>> expression)
    {
        return await _context.Set<T>()
            .Where(expression)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public IQueryable<T> GetAll()
    {
        return _context.Set<T>().AsNoTracking();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _context.Set<T>().AsNoTracking().ToListAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        _context.Set<T>().Attach(entity);
        _context.Set<T>().Entry(entity).State = EntityState.Modified;
        _context.Set<T>().Update(entity);
    }

    public Task<bool> IsExistAsync(Expression<Func<T, bool>> expression)
    {
        return _context.Set<T>().AnyAsync(expression);
    }
}