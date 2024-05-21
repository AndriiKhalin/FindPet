using System.Linq.Expressions;

namespace Interfaces.IEntityRepository;

public interface IHelperBaseRepository<T> where T : class
{
    Task<IQueryable<T>> GetAllAsync();
    Task<IQueryable<T>> GetByConditionAsync(Expression<Func<T, bool>> expression);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> expression);
    Task CreateAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}