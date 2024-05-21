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
    private readonly IUnitOfWork _unitOfWorkRep;
    private readonly IMapper _mapper;
    private readonly IManageImage<Ad> _manageImage;
    private readonly ILoggerManager _logger;


    public AdService(IUnitOfWork unitOfWorkRep, IMapper mapper, IManageImage<Ad> manageImage, ILoggerManager logger)
    {
        _unitOfWorkRep = unitOfWorkRep;
        _mapper = mapper;
        _manageImage = manageImage;
        _logger = logger;
    }

    public Task<IEnumerable<Ad>> GetAdsAsync()
    {
        return _unitOfWorkRep.Ad.GetsAsync();
    }

    public async Task<Ad?> GetAdAsync(Guid adId)
    {
        if (!await AdExistsAsync(adId))
        {
            _logger.LogError($"Ad with id: {adId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid ad Id");
        }

        return await _unitOfWorkRep.Ad.GetAsync(adId);
    }

    //public async Task<Pet?> GetPetByAd(Guid adId)
    //{
    //    if (!await AdExistsAsync(adId))
    //    {
    //        _logger.LogError($"Ad with id: {adId}, hasn't been found in db.");
    //        throw new ArgumentNullException("Invalid ad Id");
    //    }
    //    return await _unitOfWorkRep.Ad.GetPetByAd(adId);
    //}

    //public async Task<User> GetUserByAd(Guid adId)
    //{
    //    if (!await AdExistsAsync(adId))
    //    {
    //        _logger.LogError($"Ad with id: {adId}, hasn't been found in db.");
    //        throw new ArgumentNullException("Invalid ad Id");
    //    }
    //    return await _unitOfWorkRep.Ad.GetAsync(adId);
    //}

    public async Task<bool> AdExistsAsync(Guid adId)
    {
        return await _unitOfWorkRep.Ad.IsExistAsync(adId);
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

        await _unitOfWorkRep.Ad.DeleteAsync(adId);

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

        await _unitOfWorkRep.Ad.UpdateAsync(adEntity);

        await _unitOfWorkRep.Save();
    }

    public async Task<Ad> CreateAdAsync(Guid petId, Guid userId, AdForCreateDto ad)
    {
        if (petId == Guid.Empty || userId == Guid.Empty || ad == null)
        {
            _logger.LogError("Error");
            throw new ArgumentNullException("Invalid petId,userId or ad object.");
        }

        var petEntity = await _unitOfWorkRep.Pet.GetAsync(petId);
        var userEntity = await _unitOfWorkRep.User.GetAsync(userId);

        var adMap = _mapper.Map<Ad>(ad);
        adMap.UserId = userEntity.Id;
        adMap.PetId = petEntity.Id;
        adMap.Photo = await _manageImage.UploadFileAsync(ad.Photo); ;
        adMap.Date = DateTime.UtcNow;

        await _unitOfWorkRep.Ad.CreateAsync(adMap);

        await _unitOfWorkRep.Save();

        return adMap;
    }
}