using System.Linq.Expressions;
using Interfaces.IEntityRepository;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Repository.EntityRepository;

public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
{
    private readonly FindPetDbContext _context;

    public BaseRepository(FindPetDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var result = _context.Set<T>().FindAsync(id);
        _context.Set<T>().Remove(result.Result);
    }

    public async Task<IQueryable<T>> GetByConditionAsync(Expression<Func<T, bool>> expression)
    {
        return _context.Set<T>().Where(expression).AsNoTracking();
    }

    public async Task<IQueryable<T>> GetAllAsync()
    {
        return _context.Set<T>().AsNoTracking();
    }

    public async Task UpdateAsync(T entity)
    {
        _context.Set<T>().Attach(entity);
        _context.Set<T>().Entry(entity).State = EntityState.Modified;
        _context.Set<T>().Update(entity);
    }

    //public Task<bool> Exists(Guid id)
    //{
    //    //return await _context.Set<T>().AnyAsync(x => x.Id == id);
    //    return Task.FromResult(true);
    //}

    //public async Task<bool> Exists(string name)
    //{
    //    var entities = await _context.Set<T>().ToListAsync();
    //    return entities.Any(x => x.GetType().GetProperties().Any(p => p.PropertyType == typeof(string) && (string)p.GetValue(x) == name));
    //}

    public Task<bool> ExistsAsync(Expression<Func<T, bool>> expression)
    {
        return _context.Set<T>().AnyAsync(expression);
    }
}