namespace FindPet.Infrastructure.Interfaces.IEntityRepository;

public interface IBaseRepository<T> where T : class
{
    IEnumerable<T> Gets();

    Task<T?> GetAsync(Guid entityId);

    Task<bool> IsExistAsync(Guid entityId);

    Task DeleteAsync(Guid entityId);

    Task UpdateAsync(T entity);

    Task CreateAsync(T entity);
}