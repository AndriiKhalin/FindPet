using Models.Entities;

namespace Interfaces.IEntityRepository;

public interface IUserRepository<T> : IBaseRepository<T> where T : User
{
    Task<IEnumerable<T>> GetsAsync();

    Task<T?> GetAsync(Guid userId);

    //Task<IEnumerable<Ad>?> GetAdsByOwner(Guid ownerId);

    //Task<IEnumerable<Pet>?> GetPetsByOwner(Guid ownerId);

    Task<bool> IsExistAsync(Guid userId);

    Task<bool> IsExistAsync(string userFirstName);

    Task DeleteAsync(Guid userId);

    Task UpdateAsync(User user);

    Task CreateAsync(User user);
}