using AutoMapper;
using Interfaces.IEntityRepository;
using Interfaces.IEntityService;
using Interfaces.IImageService;
using Interfaces.ILoggerService;
using Models.DTO.OwnerDTO;
using Models.Entities;
using System.Security.Cryptography.Xml;

namespace Services.Service.EntityService;

public class OwnerService : IOwnerService
{
    private readonly IUnitOfWork _unitOfWorkRep;
    private readonly IMapper _mapper;
    private readonly IManageImage<Owner> _manageImage;
    private readonly ILoggerManager _logger;

    public OwnerService(IUnitOfWork unitOfWorkRep, IMapper mapper, IManageImage<Owner> manageImage, ILoggerManager logger)
    {
        _unitOfWorkRep = unitOfWorkRep;
        _mapper = mapper;
        _manageImage = manageImage;
        _logger = logger;
    }

    public Task<IEnumerable<Owner>> GetOwnersAsync()
    {
        return _unitOfWorkRep.Owner.GetsAsync();
    }

    public async Task<Owner?> GetOwnerAsync(Guid ownerId)
    {
        if (!await OwnerExistsAsync(ownerId))
        {
            _logger.LogError($"Owner with id: {ownerId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid owner Id");
        }

        return await _unitOfWorkRep.Owner.GetAsync(ownerId);
    }

    //public async Task<IEnumerable<Ad>?> GetAdsByOwnerAsync(Guid ownerId)
    //{
    //    if (!await OwnerExistsAsync(ownerId))
    //    {
    //        _logger.LogError($"Owner with id: {ownerId}, hasn't been found in db.");
    //        throw new ArgumentNullException("Invalid owner Id");
    //    }

    //    return await _unitOfWorkRep.Owner.GetAdsByOwner(ownerId);
    //}

    //public async Task<IEnumerable<Pet>?> GetPetsByOwnerAsync(Guid ownerId)
    //{
    //    if (!await OwnerExistsAsync(ownerId))
    //    {
    //        _logger.LogError($"Owner with id: {ownerId}, hasn't been found in db.");
    //        throw new ArgumentNullException("Invalid owner Id");
    //    }

    //    return await _unitOfWorkRep.Owner.GetPetsByOwner(ownerId);
    //}

    public async Task<bool> OwnerExistsAsync(Guid ownerId)
    {
        return await _unitOfWorkRep.Owner.IsExistAsync(ownerId);
    }

    public async Task<bool> OwnerExistsAsync(string ownerFirstName)
    {
        return await _unitOfWorkRep.Owner.IsExistAsync(ownerFirstName);
    }

    public async Task DeleteOwnerAsync(Guid ownerId)
    {
        if (!await OwnerExistsAsync(ownerId))
        {
            _logger.LogError($"Owner with id: {ownerId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid owner Id");
        }

        var ownerEntityForDelete = await GetOwnerAsync(ownerId);

        _manageImage.DeleteFile(ownerEntityForDelete.Photo);

        await _unitOfWorkRep.Owner.DeleteAsync(ownerId);

        await _unitOfWorkRep.Save();
    }

    public async Task UpdateOwnerAsync(Guid ownerId, OwnerForUpdateDto owner)
    {

        if (owner == null)
        {
            _logger.LogError($"Owner object sent from client is null.");
            throw new ArgumentNullException("Owner is null");
        }

        if (!await OwnerExistsAsync(ownerId))
        {
            _logger.LogError($"Owner with id: {ownerId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid owner Id");
        }

        var ownerEntity = await GetOwnerAsync(ownerId);

        if (owner.Photo is not null)
        {
            await _manageImage.UploadFileAsync(owner.Photo);
            _manageImage.DeleteFile(ownerEntity.Photo);
        }
        else
        {
            _logger.LogError($"Photo is null");
            throw new ArgumentException("Photo cannot be null.");
        }

        _mapper.Map(owner, ownerEntity);

        await _unitOfWorkRep.Owner.UpdateAsync(ownerEntity);

        await _unitOfWorkRep.Save();
    }



    public async Task<Owner> CreateOwnerAsync(OwnerForCreateDto owner)
    {
        if (owner == null)
        {
            _logger.LogError("Error");
            throw new ArgumentNullException("Invalid  owner object.");
        }

        var ownerMap = _mapper.Map<Owner>(owner);

        ownerMap.Photo = await _manageImage.UploadFileAsync(owner.Photo);


        await _unitOfWorkRep.Owner.CreateAsync(ownerMap);

        await _unitOfWorkRep.Save();

        return ownerMap;
    }
}