using AutoMapper;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.BusinessLogicLayer.Interfaces.IImageService;
using FindPet.BusinessLogicLayer.Interfaces.IMLService;
using FindPet.DataAccessLayer.Interfaces.IEntityRepository;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;
using FindPet.Domain.Entities;
using FindPet.Domain.Exceptions;
using FindPet.Domain.Interfaces.ILoggerService;
using FindPet.Media.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace FindPet.BusinessLogicLayer.Services.EntityService;

public class PetService : IPetService
{
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly ILoggerManager _logger;
    private readonly IManageImage<Pet> _manageImage;
    private readonly IMapper _mapper;
    private readonly IMediaStorageService _mediaStorageService;
    private readonly IMLService _mlService;
    private readonly IUnitOfWork _unitOfWorkRep;

    public PetService(IUnitOfWork unitOfWorkRep,
        IMapper mapper,
        IManageImage<Pet> manageImage,
        IMLService mlService,
        ILoggerManager logger,
        IWebHostEnvironment hostingEnvironment,
        IMediaStorageService mediaStorageService
    )
    {
        _unitOfWorkRep = unitOfWorkRep;
        _mapper = mapper;
        _manageImage = manageImage;
        _mlService = mlService;
        _logger = logger;
        _hostingEnvironment = hostingEnvironment;
        _mediaStorageService = mediaStorageService;
    }

    public IEnumerable<Pet> GetPets()
    {
        return _unitOfWorkRep.Pet.Gets();
    }

    public async Task<Pet?> GetPetByIdAsync(Guid petId)
    {
        if (petId == Guid.Empty) throw new BadRequestException("PetId must be a valid non-empty GUID");
        if (!await PetExistsAsync(petId))
        {
            _logger.LogError($"Pet with id: {petId}, hasn't been found in db.");
            throw new NotFoundException("Pet", petId);
        }

        return await _unitOfWorkRep.Pet.GetAsync(petId);
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

            // Predict type for new photo
            petEntity.Type = await PredictPetTypeAsync(pet.Photo);
        }

        _mapper.Map(pet, petEntity);

        await _unitOfWorkRep.Pet.UpdateAsync(petEntity);

        await _unitOfWorkRep.SaveAsync();
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

        return pet;
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
        catch (Exception ex)
        {
            _logger.LogError($"Failed to predict pet type for photo {photoPath}: {ex.Message}");
            return "Unknown";
        }
    }
}