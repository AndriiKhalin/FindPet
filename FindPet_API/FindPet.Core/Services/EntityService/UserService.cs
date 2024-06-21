using AutoMapper;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;
using FindPet.Domain.Entities;
using FindPet.Infrastructure.Interfaces.IEntityRepository;
using FindPet.Infrastructure.Interfaces.IEntityService;
using FindPet.Infrastructure.Interfaces.IImageService;
using FindPet.Infrastructure.Interfaces.ILoggerService;

namespace FindPet.Core.Services.EntityService;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWorkRep;
    private readonly IMapper _mapper;
    private readonly IManageImage<User> _manageImage;
    private readonly ILoggerManager _logger;

    public UserService(IUnitOfWork unitOfWorkRep, IMapper mapper, IManageImage<User> manageImage, ILoggerManager logger)
    {
        _unitOfWorkRep = unitOfWorkRep;
        _mapper = mapper;
        _manageImage = manageImage;
        _logger = logger;
    }

    public IEnumerable<User> GetUsers()
    {
        return _unitOfWorkRep.User.Gets();
    }

    public async Task<User?> GetUserAsync(Guid userId)
    {
        if (!await UserExistsAsync(userId))
        {
            _logger.LogError($"User with id: {userId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid user Id");
        }

        return await _unitOfWorkRep.User.GetAsync(userId);
    }

    public Task<User?> GetUserAsync(string userName)
    {
        throw new NotImplementedException();
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

    public async Task<bool> UserExistsAsync(Guid userId)
    {
        return await _unitOfWorkRep.User.IsExistAsync(userId);
    }

    public async Task<bool> UserExistsAsync(string userName)
    {
        return await _unitOfWorkRep.User.IsExistAsync(userName);
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        if (!await UserExistsAsync(userId))
        {
            _logger.LogError($"User with id: {userId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid user Id");
        }

        var userEntityForDelete = await GetUserAsync(userId);

        _manageImage.DeletePhoto(userEntityForDelete.Photo);

        await _unitOfWorkRep.User.DeleteAsync(userId);

        await _unitOfWorkRep.SaveAsync();
    }

    public async Task UpdateUserAsync(Guid userId, UserForUpdateDto user)
    {

        if (user == null)
        {
            _logger.LogError($"User object sent from client is null.");
            throw new ArgumentNullException("User is null");
        }

        if (!await UserExistsAsync(userId))
        {
            _logger.LogError($"User with id: {userId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid user Id");
        }

        var userEntity = await GetUserAsync(userId);


        if (user.Photo is not null)
        {
            _manageImage.DeletePhoto(userEntity.Photo);
            await _manageImage.UploadPhotoAsync(user.Photo, userId);

        }
        else
        {
            _logger.LogError($"Photo is null");
            throw new ArgumentException("Photo cannot be null.");
        }

        _mapper.Map(user, userEntity);


        await _unitOfWorkRep.User.UpdateAsync(userEntity);

        await _unitOfWorkRep.SaveAsync();
    }

    public async Task<User> CreateUserAsync(UserForCreateDto user)
    {
        if (user == null)
        {
            _logger.LogError("Error");
            throw new ArgumentNullException("Invalid  user object.");
        }

        var userMap = _mapper.Map<User>(user);

        userMap.DateCreateUpdate = DateTime.UtcNow;
        userMap.Photo = await _manageImage.UploadPhotoAsync(user.Photo, userMap.Id);


        await _unitOfWorkRep.User.CreateAsync(userMap);

        await _unitOfWorkRep.SaveAsync();

        return userMap;
    }
}