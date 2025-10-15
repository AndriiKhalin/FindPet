using AutoMapper;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.BusinessLogicLayer.Interfaces.IImageService;
using FindPet.DataAccessLayer.Interfaces.IEntityRepository;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;
using FindPet.Domain.Entities;
using FindPet.Domain.Exceptions;
using FindPet.Domain.Interfaces.ILoggerService;
using Microsoft.EntityFrameworkCore;

namespace FindPet.BusinessLogicLayer.Services.EntityService;

public class UserService(
    IUnitOfWork unitOfWorkRep,
    IMapper mapper,
    IManageImage<User> manageImage,
    ILoggerManager logger)
    : IUserService
{
    public IEnumerable<User> GetUsers()
    {
        return unitOfWorkRep.User.Gets();
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        if (userId == Guid.Empty) throw new BadRequestException("User ID must be NON-Empty");

        if (!await UserExistsAsync(userId))
        {
            logger.LogError($"User with id: {userId}, hasn't been found in db.");
            throw new NotFoundException("User", userId);
        }

        return await unitOfWorkRep.User.GetAsync(userId);
    }

    public async Task<User?> GetUserByNameAsync(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName)) throw new BadRequestException("UserName cannot be empty");

        if (!await UserExistsAsync(userName))
        {
            logger.LogError($"User with name: {userName}, hasn't been found in db.");
            throw new NotFoundException("User", userName);
        }

        return await unitOfWorkRep.User.GetUserAsync(userName);
    }

    //public async Task<IEnumerable<Ad>?> GetAdsByUserAsync(Guid userId)
    //{
    //    if (!await UserExistsAsync(userId))
    //    {
    //        _logger.LogError($"User with id: {userId}, hasn't been found in db.");
    //        throw new ArgumentNullException("Invalid user Id");
    //    }

    //    return await _unitOfWorkRep.User.GetAdsByUser(userId);
    //}

    //public async Task<IEnumerable<Pet>?> GetPetsByUserAsync(Guid userId)
    //{
    //    if (!await UserExistsAsync(userId))
    //    {
    //        _logger.LogError($"User with id: {userId}, hasn't been found in db.");
    //        throw new ArgumentNullException("Invalid user Id");
    //    }

    //    return await _unitOfWorkRep.User.GetPetsByUser(userId);
    //}
    public async Task<bool> IsEmailRegisteredAsync(string email)
    {
        return await unitOfWorkRep.User.IsExistAsync(x => x.Email != null && EF.Functions.Like(x.Email, email));
    }

    public async Task<bool> IsPhoneNumberRegisteredAsync(string phoneNumber)
    {
        return await unitOfWorkRep.User.IsExistAsync(x =>
            x.PhoneNumber != null && EF.Functions.Like(x.PhoneNumber, phoneNumber));
    }

    public async Task<bool> UserExistsAsync(Guid userId)
    {
        return await unitOfWorkRep.User.IsExistAsync(userId);
    }

    public async Task<bool> UserExistsAsync(string userName)
    {
        return await unitOfWorkRep.User.IsExistAsync(userName);
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        if (!await UserExistsAsync(userId))
        {
            logger.LogError($"User with id: {userId}, hasn't been found in db.");
            throw new NotFoundException("User", userId);
        }

        var userEntityForDelete = await GetUserByIdAsync(userId);

        manageImage.DeletePhoto(userEntityForDelete.Photo);

        await unitOfWorkRep.User.DeleteAsync(userId);

        await unitOfWorkRep.SaveAsync();
    }

    public async Task UpdateUserAsync(Guid userId, UserForUpdateDto user)
    {
        if (user == null)
        {
            logger.LogError("User object sent from client is null.");
            throw new BadRequestException("User is null");
        }

        if (!await UserExistsAsync(userId))
        {
            logger.LogError($"User with id: {userId}, hasn't been found in db.");
            throw new NotFoundException("User", userId);
        }

        var userEntity = await GetUserByIdAsync(userId);

        if (user.Photo is not null)
        {
            manageImage.DeletePhoto(userEntity.Photo);
            await manageImage.UploadPhotoAsync(user.Photo, userId);
        }

        mapper.Map(user, userEntity);

        await unitOfWorkRep.User.UpdateAsync(userEntity);

        await unitOfWorkRep.SaveAsync();
    }

    public async Task<User> CreateUserAsync(UserForCreateDto user)
    {
        if (user == null)
        {
            logger.LogError("Error");
            throw new BadRequestException("Invalid  user object.");
        }

        var userMap = mapper.Map<User>(user);

        userMap.DateCreateUpdate = DateTime.UtcNow;
        userMap.Photo = user.Photo;

        await unitOfWorkRep.User.CreateAsync(userMap);

        await unitOfWorkRep.SaveAsync();

        return userMap;
    }
}