using AutoMapper;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.BusinessLogicLayer.Interfaces.IImageService;
using FindPet.BusinessLogicLayer.Interfaces.ILoggerService;
using FindPet.BusinessLogicLayer.Interfaces.IMLService;
using FindPet.DataAccessLayer.Interfaces.IEntityRepository;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;
using FindPet.Domain.Entities;
using FindPet.Domain.Exceptions;

namespace FindPet.BusinessLogicLayer.Services.EntityService;

public class PetService : IPetService
{
    private readonly ILoggerManager _logger;
    private readonly IManageImage<Pet> _manageImage;
    private readonly IMapper _mapper;
    private readonly IMLService _mlService;
    private readonly IUnitOfWork _unitOfWorkRep;

    public PetService(IUnitOfWork unitOfWorkRep, IMapper mapper, IManageImage<Pet> manageImage, IMLService mlService,
        ILoggerManager logger)
    {
        _unitOfWorkRep = unitOfWorkRep;
        _mapper = mapper;
        _manageImage = manageImage;
        _mlService = mlService;
        _logger = logger;
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

        _manageImage.DeletePhoto(petEntityForDelete.Photo);

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

        if (pet.Photo is not null)
        {
            _manageImage.DeletePhoto(petEntity.Photo);
            await _manageImage.UploadPhotoAsync(pet.Photo, petId);
        }

        _mapper.Map(pet, petEntity);

        await _unitOfWorkRep.Pet.UpdateAsync(petEntity);

        await _unitOfWorkRep.SaveAsync();
    }

    public async Task<Pet> CreatePetAsync(Guid userId, PetForCreateDto createPetDto)
    {
        var exceptions = new List<ValidationError>();
        if (userId == Guid.Empty || createPetDto == null)
        {
            _logger.LogError("Error");
            throw new BadRequestException("Invalid userId or createPetDto object.");
        }

        if (string.IsNullOrWhiteSpace(createPetDto.Nickname))
            exceptions.Add(new ValidationError("Nickname", "Nickname is required"));

        if (string.IsNullOrWhiteSpace(createPetDto.Breed))
            exceptions.Add(new ValidationError("Breed", "Breed is required"));

        if (exceptions.Any()) throw new ValidationException(exceptions);

        if (!await _unitOfWorkRep.User.IsExistAsync(userId)) throw new NotFoundException("User", userId);

        var pet = _mapper.Map<Pet>(createPetDto);

        pet.UserId = userId;
        pet.DateCreateUpdate = DateTime.UtcNow;
        pet.Photo = createPetDto.Photo;
        pet.Type = !string.IsNullOrEmpty(pet.Photo)
            ? await _mlService.PredictAsync(Path.Join(@"wwwroot", pet.Photo.TrimStart('/', '\\')))
            : "Unknown";

        await _unitOfWorkRep.Pet.CreateAsync(pet);

        await _unitOfWorkRep.SaveAsync();

        return pet;
    }
}