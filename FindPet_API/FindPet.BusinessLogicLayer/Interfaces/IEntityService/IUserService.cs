using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;
using FindPet.Domain.Entities;

namespace FindPet.BusinessLogicLayer.Interfaces.IEntityService;

public interface IUserService
{
    IEnumerable<User> GetUsers();

    Task<User?> GetUserByIdAsync(Guid UserId);

    Task<User?> GetUserByNameAsync(string userName);

    Task<bool> UserExistsAsync(Guid UserId);

    Task<bool> UserExistsAsync(string UserFirstName);

    Task<bool> IsEmailRegisteredAsync(string email);

    Task<bool> IsPhoneNumberRegisteredAsync(string phoneNumber);

    Task DeleteUserAsync(Guid UserId);

    Task UpdateUserAsync(Guid UserId, UserForUpdateDto User);

    Task<User> CreateUserAsync(UserForCreateDto User);
}