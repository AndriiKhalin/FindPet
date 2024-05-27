using System.Linq.Expressions;

namespace FindPet.Infrastructure.Interfaces.IEntityRepository;

public interface IHelperBaseRepository<T> where T : class
{
    IQueryable<T> GetAll();
    Task<IQueryable<T>> GetByConditionAsync(Expression<Func<T, bool>> expression);
    Task<bool> IsExistAsync(Expression<Func<T, bool>> expression);
    Task CreateAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}