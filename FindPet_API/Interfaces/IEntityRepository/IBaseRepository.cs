using Models.Entities;

namespace Interfaces.IEntityRepository;

public interface IBaseRepository<T> where T : class
{
    Task<IEnumerable<T>> GetsAsync();

    Task<T?> GetAsync(Guid entityId);

    Task<bool> IsExistAsync(Guid entityId);

    Task DeleteAsync(Guid entityId);

    Task UpdateAsync(T entity);

    Task CreateAsync(T entity);
}