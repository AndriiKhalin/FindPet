using System.Linq.Expressions;

namespace FindPet.DataAccessLayer.Interfaces.IEntityRepository;

public interface IHelperBaseRepository<T> where T : class
{
    IQueryable<T> GetAll();

    Task<IEnumerable<T>> GetAllAsync();

    IQueryable<T> GetByCondition(Expression<Func<T, bool>> expression);
    Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> expression);
    Task<T?> GetSingleByConditionAsync(Expression<Func<T, bool>> expression);

    Task<bool> IsExistAsync(Expression<Func<T, bool>> expression);

    Task CreateAsync(T entity);

    Task UpdateAsync(T entity);

    Task DeleteAsync(Guid id);
}