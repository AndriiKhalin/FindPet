using AutoMapper;
using Interfaces.IEntityRepository;
using Interfaces.IEntityService;
using Interfaces.IImageService;
using Interfaces.ILoggerService;
using Models.DTO.AdDTO;
using Models.Entities;
using System.Security.Cryptography.Xml;

namespace Services.Service.EntityService;

public class AdService : IAdService
{
    private readonly IUnitOfWorkRepository _unitOfWorkRep;
    private readonly IMapper _mapper;
    private readonly IManageImage<Pet> _manageImage;
    private readonly ILoggerManager _logger;


    public AdService(IUnitOfWorkRepository unitOfWorkRep, IMapper mapper, IManageImage<Pet> manageImage, ILoggerManager logger)
    {
        _unitOfWorkRep = unitOfWorkRep;
        _mapper = mapper;
        _manageImage = manageImage;
        _logger = logger;
    }

    public async Task<IEnumerable<Ad>> GetAdsAsync()
    {
        return await _unitOfWorkRep.Ad.GetAdsAsync();
    }

    public async Task<Ad?> GetAdAsync(Guid adId)
    {
        if (!await AdExistsAsync(adId))
        {
            _logger.LogError($"Ad with id: {adId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid ad Id");
        }

        return await _unitOfWorkRep.Ad.GetAdAsync(adId);
    }

    public async Task<Pet?> GetPetByAd(Guid adId)
    {
        if (!await AdExistsAsync(adId))
        {
            _logger.LogError($"Ad with id: {adId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid ad Id");
        }
        return await _unitOfWorkRep.Ad.GetPetByAd(adId);
    }

    public async Task<User> GetUserByAd(Guid adId)
    {
        if (!await AdExistsAsync(adId))
        {
            _logger.LogError($"Ad with id: {adId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid ad Id");
        }
        return await _unitOfWorkRep.Ad.GetUserByAd(adId);
    }

    public async Task<bool> AdExistsAsync(Guid adId)
    {
        return await _unitOfWorkRep.Ad.AdExistsAsync(adId);
    }

    public async Task DeleteAdAsync(Guid adId)
    {
        if (!await AdExistsAsync(adId))
        {
            _logger.LogError($"Ad with id: {adId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid ad Id");
        }

        var adEntityForDelete = await GetAdAsync(adId);

        _manageImage.DeleteFile(adEntityForDelete.Photo);

        await _unitOfWorkRep.Ad.DeleteAdAsync(adId);

        await _unitOfWorkRep.Save();
    }

    public async Task UpdateAdAsync(Guid adId, AdForUpdateDto ad)
    {

        if (ad == null)
        {
            _logger.LogError($"Ad object sent from client is null.");
            throw new ArgumentNullException("Ad is null");
        }

        if (!await AdExistsAsync(adId))
        {
            _logger.LogError($"Ad with id: {adId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid ad Id");
        }

        var adEntity = await GetAdAsync(adId);

        if (ad.Photo is not null)
        {
            await _manageImage.UploadFileAsync(ad.Photo);
            _manageImage.DeleteFile(adEntity.Photo);
        }
        else
        {
            _logger.LogError($"Photo is null");
            throw new ArgumentException("Photo cannot be null.");
        }

        _mapper.Map(ad, adEntity);

        await _unitOfWorkRep.Ad.UpdateAdAsync(adEntity);

        await _unitOfWorkRep.Save();
    }

    public Task<Ad> CreateAdAsync(Guid petId, Guid userId, AdForCreateDto ad)
    {
        throw new NotImplementedException();
    }

    //public async Task<Ad> CreateAdAsync(Guid petId, Guid userId, AdForCreateDto ad)
    //{
    //    if (petId == Guid.Empty || userId == Guid.Empty || ad == null)
    //    {
    //        _logger.LogError("Error");
    //        throw new ArgumentNullException("Invalid petId,userId or ad object.");
    //    }

    //    var petEntity = await _unitOfWorkRep.Pet.GetPetAsync(petId);
    //    //var userEntity = await _unitOfWorkRep.User.Get(petId);

    //    var transportMap = _mapper.Map<Transport>(transport);
    //    transportMap.TransportCategoryId = categoryEntity.Id;
    //    transportMap.ImgUrl = await _manageImage.UploadFileAsync(transport.ImgUrl); ;
    //    transportMap.CreatedUpdatedAt = DateTime.UtcNow;

    //    await _unitOfWorkRep.Transport.CreateTransport(transportMap);

    //    await _unitOfWorkRep.Save();

    //    return transportMap;
    //}
}