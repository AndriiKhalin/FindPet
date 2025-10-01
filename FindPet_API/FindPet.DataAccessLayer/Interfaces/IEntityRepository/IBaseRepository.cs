using System.Linq.Expressions;

namespace FindPet.DataAccessLayer.Interfaces.IEntityRepository;

public interface IBaseRepository<T> where T : class
{
    IEnumerable<T> Gets();

    Task<T?> GetAsync(Guid entityId);

    Task<bool> IsExistAsync(Guid entityId);

    Task<bool> IsExistAsync(Expression<Func<T, bool>> expression);

    Task DeleteAsync(Guid entityId);

    Task UpdateAsync(T entity);

    Task CreateAsync(T entity);
}