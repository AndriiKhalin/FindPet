using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;
using FindPet.Domain.Entities;

namespace FindPet.Infrastructure.Interfaces.IEntityService;

public interface IUserService
{
    IEnumerable<User> GetUsers();

    Task<User?> GetUserAsync(Guid UserId);

    Task<User?> GetUserAsync(string userName);

    Task<bool> UserExistsAsync(Guid UserId);

    Task<bool> UserExistsAsync(string UserFirstName);

    Task DeleteUserAsync(Guid UserId);

    Task UpdateUserAsync(Guid UserId, UserForUpdateDto User);

    Task<User> CreateUserAsync(UserForCreateDto User);
}