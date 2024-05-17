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
    private readonly IUnitOfWorkRepository _unitOfWorkRep;
    private readonly IMapper _mapper;
    private readonly IManageImage<Finder> _manageImage;
    private readonly ILoggerManager _logger;

    public OwnerService(IUnitOfWorkRepository unitOfWorkRep, IMapper mapper, IManageImage<Finder> manageImage, ILoggerManager logger)
    {
        _unitOfWorkRep = unitOfWorkRep;
        _mapper = mapper;
        _manageImage = manageImage;
        _logger = logger;
    }

    public async Task<IEnumerable<Owner>> GetOwnersAsync()
    {
        return await _unitOfWorkRep.Owner.GetOwnersAsync();
    }

    public async Task<Owner?> GetOwnerAsync(Guid ownerId)
    {
        if (!await OwnerExistsAsync(ownerId))
        {
            _logger.LogError($"Owner with id: {ownerId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid owner Id");
        }

        return await _unitOfWorkRep.Owner.GetOwnerAsync(ownerId);
    }

    public async Task<IEnumerable<Ad>?> GetAdsByOwnerAsync(Guid ownerId)
    {
        if (!await OwnerExistsAsync(ownerId))
        {
            _logger.LogError($"Owner with id: {ownerId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid owner Id");
        }

        return await _unitOfWorkRep.Owner.GetAdsByOwner(ownerId);
    }

    public async Task<IEnumerable<Pet>?> GetPetsByOwnerAsync(Guid ownerId)
    {
        if (!await OwnerExistsAsync(ownerId))
        {
            _logger.LogError($"Owner with id: {ownerId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid owner Id");
        }

        return await _unitOfWorkRep.Owner.GetPetsByOwner(ownerId);
    }

    public async Task<bool> OwnerExistsAsync(Guid ownerId)
    {
        return await _unitOfWorkRep.Finder.FinderExistsAsync(ownerId);
    }

    public async Task<bool> OwnerExistsAsync(string ownerFirstName)
    {
        return await _unitOfWorkRep.Finder.FinderExistsAsync(ownerFirstName);
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

        await _unitOfWorkRep.Owner.DeleteOwnerAsync(ownerId);

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

        await _unitOfWorkRep.Owner.UpdateOwnerAsync(ownerEntity);

        await _unitOfWorkRep.Save();
    }

    public Task CreateOwnerAsync(OwnerForCreateDto owner)
    {
        throw new NotImplementedException();
    }

    //public async Task CreateOwnerAsync(OwnerForCreateDto owner)
    //{
    //    if (owner == null)
    //    {
    //        _logger.LogError("Error");
    //        throw new ArgumentNullException("Invalid  owner object.");
    //    }

    //    var ownerEntity = await _unitOfWorkRep..GetCategory(categoryId);

    //    var transportMap = _mapper.Map<Transport>(transport);
    //    transportMap.TransportCategoryId = categoryEntity.Id;
    //    transportMap.ImgUrl = await _manageImage.UploadFileAsync(transport.ImgUrl); ;
    //    transportMap.CreatedUpdatedAt = DateTime.UtcNow;

    //    await _unitOfWorkRep.Transport.CreateTransport(transportMap);

    //    await _unitOfWorkRep.Save();

    //    return transportMap;
    //}
}