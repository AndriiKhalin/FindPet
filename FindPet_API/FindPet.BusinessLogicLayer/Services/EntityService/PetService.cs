using AutoMapper;
using FindPet.BusinessLogicLayer.Interfaces.ICacheService;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.BusinessLogicLayer.Interfaces.IImageService;
using FindPet.BusinessLogicLayer.Interfaces.IMLService;
using FindPet.BusinessLogicLayer.Interfaces.INotificationService;
using FindPet.BusinessLogicLayer.Interfaces.IPetMatchingService;
using FindPet.DataAccessLayer.Interfaces.IEntityRepository;
using FindPet.Domain.Constants;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;
using FindPet.Domain.Entities;
using FindPet.Domain.Exceptions;
using FindPet.Domain.Interfaces.ILoggerService;
using FindPet.Media.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace FindPet.BusinessLogicLayer.Services.EntityService;

public class PetService : IPetService
{
    private readonly IRedisCacheService _cache;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly ILoggerManager _logger;
    private readonly IManageImage<Pet> _manageImage;
    private readonly IMapper _mapper;
    private readonly IMediaStorageService _mediaStorageService;
    private readonly IMLService _mlService;
    private readonly INotificationService _notificationService;
    private readonly IPetMatchingService _petMatchingService;
    private readonly IUnitOfWork _unitOfWorkRep;

    public PetService(IUnitOfWork unitOfWorkRep,
        IMapper mapper,
        IManageImage<Pet> manageImage,
        IMLService mlService,
        ILoggerManager logger,
        IWebHostEnvironment hostingEnvironment,
        IMediaStorageService mediaStorageService,
        IRedisCacheService cache,
        INotificationService notificationService,
        IPetMatchingService petMatchingService)
    {
        _unitOfWorkRep = unitOfWorkRep;
        _mapper = mapper;
        _manageImage = manageImage;
        _mlService = mlService;
        _logger = logger;
        _hostingEnvironment = hostingEnvironment;
        _mediaStorageService = mediaStorageService;
        _cache = cache;
        _notificationService = notificationService;
        _petMatchingService = petMatchingService;
    }

    public async Task<IEnumerable<Pet>> GetPetsAsync()
    {
        return await _cache.GetValueOrInitializeAsync(
            CacheKeys.AllPets,
            async () => await _unitOfWorkRep.Pet.GetsAsync(),
            CacheKeys.Duration.Short
        );
    }

    public async Task<Pet?> GetPetByIdAsync(Guid petId)
    {
        if (petId == Guid.Empty) throw new BadRequestException("PetId must be a valid non-empty GUID");

        var cacheKey = CacheKeys.GetPetByIdKey(petId);

        var pet = await _cache.GetValueOrInitializeAsync(
            cacheKey,
            async () => await _unitOfWorkRep.Pet.GetAsync(petId),
            CacheKeys.Duration.Medium
        );

        if (pet == null)
        {
            _logger.LogError($"Pet with id: {petId}, hasn't been found in db.");
            throw new NotFoundException("Pet", petId);
        }

        return pet;
    }

    //public async Task<IEnumerable<Ad>?> GetAdsByPetAsync(Guid petId)
    //{
    //    if (!await PetExistsAsync(petId))
    //    {
    //        _logger.LogError($"Pet with id: {petId}, hasn't been found in db.");
    //        throw new ArgumentNullException("Invalid pet Id");
    //    }

    //    return await _unitOfWorkRep.Pet.GetAdsByPet(petId);
    //}

    //public async Task<Finder> GetFinderByPetAsync(Guid petId)
    //{
    //    if (!await PetExistsAsync(petId))
    //    {
    //        _logger.LogError($"Pet with id: {petId}, hasn't been found in db.");
    //        throw new ArgumentNullException("Invalid pet Id");
    //    }

    //    return await _unitOfWorkRep.Pet.GetFinderByPet(petId);
    //}

    //public async Task<Owner> GetOwnerByPetAsync(Guid petId)
    //{
    //    if (!await PetExistsAsync(petId))
    //    {
    //        _logger.LogError($"Pet with id: {petId}, hasn't been found in db.");
    //        throw new ArgumentNullException("Invalid pet Id");
    //    }

    //    return await _unitOfWorkRep.Pet.GetOwnerByPet(petId);
    //}

    public async Task<bool> PetExistsAsync(Guid petId)
    {
        return await _unitOfWorkRep.Pet.IsExistAsync(petId);
    }

    public async Task<bool> PetExistsAsync(string petName)
    {
        return await _unitOfWorkRep.Pet.IsExistAsync(petName);
    }

    public async Task DeletePetAsync(Guid petId)
    {
        if (petId == Guid.Empty) throw new BadRequestException("PetId must be a valid non-empty GUID");

        if (!await PetExistsAsync(petId))
        {
            _logger.LogError($"Pet with id: {petId}, hasn't been found in db.");
            throw new NotFoundException("Pet", petId);
        }

        var petEntityForDelete = await GetPetByIdAsync(petId);

        if (!string.IsNullOrEmpty(petEntityForDelete.Photo))
            await _mediaStorageService.DeleteFileAsync(petEntityForDelete.Photo);

        await _unitOfWorkRep.Pet.DeleteAsync(petId);
        await _unitOfWorkRep.SaveAsync();

        await InvalidatePetCaches(petId, petEntityForDelete.UserId);
    }

    public async Task UpdatePetAsync(Guid petId, PetForUpdateDto pet)
    {
        if (petId == Guid.Empty) throw new BadRequestException("PetId must be a valid non-empty GUID");

        if (pet == null)
        {
            _logger.LogError("Pet object sent from client is null.");
            throw new BadRequestException("Pet is null");
        }

        if (!await PetExistsAsync(petId))
        {
            _logger.LogError($"Pet with id: {petId}, hasn't been found in db.");
            throw new NotFoundException("Pet", petId);
        }

        var petEntity = await GetPetByIdAsync(petId);

        if (!string.IsNullOrEmpty(pet.Photo) &&
            !string.IsNullOrEmpty(petEntity.Photo) &&
            pet.Photo != petEntity.Photo)
        {
            await _mediaStorageService.DeleteFileAsync(petEntity.Photo);
            petEntity.Type = await PredictPetTypeAsync(pet.Photo);
        }

        _mapper.Map(pet, petEntity);

        await _unitOfWorkRep.Pet.UpdateAsync(petEntity);
        await _unitOfWorkRep.SaveAsync();

        await InvalidatePetCaches(petId, petEntity.UserId);

        await CheckAndNotifyMatchesAsync(petEntity!);
    }

    public async Task<Pet> CreatePetAsync(Guid userId, PetForCreateDto createPetDto)
    {
        if (userId == Guid.Empty || createPetDto == null)
        {
            _logger.LogError("Error");
            throw new BadRequestException("Invalid userId or createPetDto object.");
        }

        if (!await _unitOfWorkRep.User.IsExistAsync(userId)) throw new NotFoundException("User", userId);

        var pet = _mapper.Map<Pet>(createPetDto);

        pet.UserId = userId;
        pet.DateCreateUpdate = DateTime.UtcNow;

        //if (createPetDto.Photo != null)
        //{
        //    pet.Photo = await _mediaStorageService.UploadFileAsync(createPetDto.Photo, pet.Id, "pets");
        //}

        pet.Type = await PredictPetTypeAsync(createPetDto.Photo);

        await _unitOfWorkRep.Pet.CreateAsync(pet);
        await _unitOfWorkRep.SaveAsync();

        await InvalidatePetCaches(null, userId);

        // Send broadcast notification about new pet
        await _notificationService.SendToAllAsync(
            $"A new {pet.Type ?? "pet"} named '{pet.Nickname}' has been posted!");

        // Check for matches and notify owners
        await CheckAndNotifyMatchesAsync(pet);

        return pet;
    }

    /// <summary>
    /// Checks for potential pet matches and sends notifications to owners.
    /// </summary>
    private async Task CheckAndNotifyMatchesAsync(Pet newPet)
    {
        try
        {
            var matches = await _petMatchingService.FindMatchesAsync(newPet);

            foreach (var matchedPet in matches)
            {
                if (matchedPet.UserId.HasValue)
                {
                    // Send notification to the owner of the matching pet
                    await _notificationService.NotifyPetOwnerAsync(
                        matchedPet.UserId.Value.ToString(),
                        matchedPet.Nickname ?? "your pet",
                        newPet.Id);

                    // Also send a general match notification
                    await _notificationService.SendMatchNotificationAsync(
                        matchedPet.Id,
                        newPet.Id,
                        $"Potential match found between '{matchedPet.Nickname}' and '{newPet.Nickname}'!");
                }
            }

            if (matches.Any())
            {
                _logger.LogInfo($"Sent {matches.Count()} match notifications for new pet {newPet.Id}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking matches for pet {newPet.Id}: {ex.Message}");
            // Don't throw - notification failure shouldn't fail pet creation
        }
    }

    /// <summary>
    ///     Downloads image from Azure Blob Storage and predicts pet type using ML model
    /// </summary>
    /// <param name="photoPath">Blob storage path/URL of the pet photo</param>
    /// <returns>Predicted pet type or "Unknown" if prediction fails</returns>
    private async Task<string> PredictPetTypeAsync(string? photoPath)
    {
        if (string.IsNullOrEmpty(photoPath))
        {
            _logger.LogWarn("Photo path is null or empty. Cannot predict pet type.");
            return "Unknown";
        }

        try
        {
            // Check if file exists in Azure Blob Storage
            var fileExists = await _mediaStorageService.FileExistsAsync(photoPath);

            if (!fileExists)
            {
                _logger.LogWarn($"Photo file not found in storage: {photoPath}");
                return "Unknown";
            }

            // Download image stream from Azure Blob Storage
            using var imageStream = await _mediaStorageService.GetFileAsync(photoPath);

            if (imageStream == null || imageStream == Stream.Null)
            {
                _logger.LogWarn($"Failed to download image stream for: {photoPath}");
                return "Unknown";
            }

            // Predict pet type using ML service
            var predictedType = await _mlService.PredictAsync(imageStream);

            _logger.LogInfo($"Pet type predicted successfully: {predictedType} for photo: {photoPath}");

            return predictedType;
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError($"Photo file not found in storage: {photoPath}. {ex.Message}");
            return "Unknown";
        }
        catch (ArgumentException ex)
        {
            _logger.LogError($"Invalid argument for photo {photoPath}: {ex.Message}");
            return "Unknown";
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError($"Operation failed for photo {photoPath}: {ex.Message}");
            return "Unknown";
        }
        catch (Exception)
        {
            _logger.LogError("Failed to predict pet type");
            return "Unknown";
        }
    }

    private async Task InvalidatePetCaches(Guid? petId, Guid? userId)
    {
        // Always invalidate list caches
        await _cache.RemoveAsync(CacheKeys.AllPets);
        await _cache.RemoveAsync(CacheKeys.RecentPets);

        // Invalidate specific pet cache
        if (petId.HasValue) await _cache.RemoveAsync(CacheKeys.GetPetByIdKey(petId.Value));

        // Invalidate user-specific pet caches
        if (userId.HasValue) await _cache.RemoveAsync(CacheKeys.GetPetsByUserKey(userId.Value));
    }
}