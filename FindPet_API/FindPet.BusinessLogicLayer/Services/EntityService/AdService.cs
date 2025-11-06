using AutoMapper;
using FindPet.BusinessLogicLayer.Interfaces.ICacheService;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.BusinessLogicLayer.Interfaces.IImageService;
using FindPet.DataAccessLayer.Interfaces.IEntityRepository;
using FindPet.Domain.Constants;
using FindPet.Domain.DTOs.EntitiesDTOs.AdDTO;
using FindPet.Domain.Entities;
using FindPet.Domain.Exceptions;
using FindPet.Domain.Interfaces.ILoggerService;
using FindPet.Media.Interfaces;

namespace FindPet.BusinessLogicLayer.Services.EntityService;

public class AdService : IAdService
{
    private readonly IRedisCacheService _cache;
    private readonly ILoggerManager _logger;
    private readonly IManageImage<Ad> _manageImage;
    private readonly IMapper _mapper;
    private readonly IMediaStorageService _mediaStorageService;
    private readonly IUnitOfWork _unitOfWorkRep;

    public AdService(IUnitOfWork unitOfWorkRep, IMapper mapper, IManageImage<Ad> manageImage, ILoggerManager logger,
        IMediaStorageService mediaStorageService,
        IRedisCacheService cache)
    {
        _unitOfWorkRep = unitOfWorkRep;
        _mapper = mapper;
        _manageImage = manageImage;
        _logger = logger;
        _mediaStorageService = mediaStorageService;
        _cache = cache;
    }

    public async Task<IEnumerable<Ad>> GetAdsAsync()
    {
        return await _cache.GetValueOrInitializeAsync(
            CacheKeys.AllAds,
            async () => await _unitOfWorkRep.Ad.GetsAsync(),
            CacheKeys.Duration.Short
        );
    }

    public async Task<Ad?> GetAdAsync(Guid adId)
    {
        //TODO: Check How to handle when we have incorrect Guid and cannot find Ad
        if (adId == Guid.Empty) throw new BadRequestException("AdId must be a valid non-empty GUID");


        var cacheKey = CacheKeys.GetAdByIdKey(adId);


        var ad = await _cache.GetValueOrInitializeAsync(
            cacheKey,
            async () => await _unitOfWorkRep.Ad.GetAsync(adId),
            CacheKeys.Duration.Medium
        );

        if (ad == null)
        {
            _logger.LogError($"Ad with id: {adId}, hasn't been found in db.");
            throw new NotFoundException("Ad", adId);
        }

        return ad;
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
            throw new NotFoundException("Ad", adId);
        }

        var adEntityForDelete = await GetAdAsync(adId);

        if (!string.IsNullOrEmpty(adEntityForDelete.Photo))
            await _mediaStorageService.DeleteFileAsync(adEntityForDelete.Photo);

        await _unitOfWorkRep.Ad.DeleteAsync(adId);
        await _unitOfWorkRep.SaveAsync();

        await InvalidateAdCaches(adId, adEntityForDelete.UserId);
    }

    public async Task UpdateAdAsync(Guid adId, AdForUpdateDto ad)
    {
        if (adId == Guid.Empty)
        {
            _logger.LogError("AdId is empty.");
            throw new BadRequestException("AdId must be a valid non-empty GUID");
        }

        if (ad == null)
        {
            _logger.LogError("Ad object sent from client is null.");
            throw new BadRequestException("Ad is null");
        }

        if (!await AdExistsAsync(adId))
        {
            _logger.LogError($"Ad with id: {adId}, hasn't been found in db.");
            throw new NotFoundException("Ad", adId);
        }

        var adEntity = await GetAdAsync(adId);

        if (!string.IsNullOrEmpty(ad.Photo) && !string.IsNullOrEmpty(adEntity.Photo) && ad.Photo != adEntity.Photo)
            await _mediaStorageService.DeleteFileAsync(adEntity.Photo);

        _mapper.Map(ad, adEntity);

        await _unitOfWorkRep.Ad.UpdateAsync(adEntity);
        await _unitOfWorkRep.SaveAsync();

        await InvalidateAdCaches(adId, adEntity.UserId);
    }

    public async Task<Ad> CreateAdAsync(Guid petId, Guid userId, AdForCreateDto ad)
    {
        if (petId == Guid.Empty || userId == Guid.Empty || ad == null)
        {
            _logger.LogError("Invalid petId,userId or ad object.");
            throw new BadRequestException("Invalid petId,userId or ad object.");
        }

        var petEntity = await _unitOfWorkRep.Pet.GetAsync(petId);
        var userEntity = await _unitOfWorkRep.User.GetAsync(userId);

        var adMap = _mapper.Map<Ad>(ad);
        adMap.UserId = userEntity.Id;
        adMap.PetId = petEntity.Id;
        adMap.DateCreateUpdate = DateTime.UtcNow;

        await _unitOfWorkRep.Ad.CreateAsync(adMap);
        await _unitOfWorkRep.SaveAsync();

        await InvalidateAdCaches(null, userId);

        return adMap;
    }

    private async Task InvalidateAdCaches(Guid? adId, Guid? userId)
    {
        await _cache.RemoveAsync(CacheKeys.AllAds);
        await _cache.RemoveAsync(CacheKeys.RecentAds);

        if (adId.HasValue) await _cache.RemoveAsync(CacheKeys.GetAdByIdKey(adId.Value));

        if (userId.HasValue) await _cache.RemoveAsync(CacheKeys.GetAdsByUserKey(userId.Value));
    }
}