using Models.Entities;

namespace Interfaces.IEntityRepository;

public interface IUserRepository<T> : IBaseRepository<T> where T : User
{
    Task<bool> IsExistAsync(string userFirstName);
}