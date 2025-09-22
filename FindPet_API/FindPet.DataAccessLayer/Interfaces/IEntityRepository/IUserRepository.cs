using FindPet.Domain.Entities;

namespace FindPet.DataAccessLayer.Interfaces.IEntityRepository;

public interface IUserRepository<T> : IBaseRepository<T> where T : User
{
    Task<bool> IsExistAsync(string userFirstName);

    Task<T?> GetUserAsync(string userName);
}