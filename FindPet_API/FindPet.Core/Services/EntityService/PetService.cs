using AutoMapper;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;
using FindPet.Domain.Entities;
using FindPet.Infrastructure.Interfaces.IEntityRepository;
using FindPet.Infrastructure.Interfaces.IEntityService;
using FindPet.Infrastructure.Interfaces.IImageService;
using FindPet.Infrastructure.Interfaces.ILoggerService;

namespace FindPet.Core.Services.EntityService;
public class PetService : IPetService
{
    private readonly IUnitOfWork _unitOfWorkRep;
    private readonly IMapper _mapper;
    private readonly IManageImage<Pet> _manageImage;
    private readonly ILoggerManager _logger;

    public PetService(IUnitOfWork unitOfWorkRep, IMapper mapper, IManageImage<Pet> manageImage, ILoggerManager logger)
    {
        _unitOfWorkRep = unitOfWorkRep;
        _mapper = mapper;
        _manageImage = manageImage;
        _logger = logger;
    }

    public IEnumerable<Pet> GetPets()
    {
        return _unitOfWorkRep.Pet.Gets();
    }

    public async Task<Pet?> GetPetAsync(Guid petId)
    {
        if (!await PetExistsAsync(petId))
        {
            _logger.LogError($"Pet with id: {petId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid pet Id");
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
        if (!await PetExistsAsync(petId))
        {
            _logger.LogError($"Pet with id: {petId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid pet Id");
        }

        var petEntityForDelete = await GetPetAsync(petId);

        _manageImage.DeletePhoto(petEntityForDelete.Photo);

        await _unitOfWorkRep.Pet.DeleteAsync(petId);

        await _unitOfWorkRep.SaveAsync();
    }

    public async Task UpdatePetAsync(Guid petId, PetForUpdateDto pet)
    {
        if (pet == null)
        {
            _logger.LogError($"Pet object sent from client is null.");
            throw new ArgumentNullException("Pet is null");
        }

        if (!await PetExistsAsync(petId))
        {
            _logger.LogError($"Pet with id: {petId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid pet Id");
        }

        var petEntity = await GetPetAsync(petId);


        if (pet.Photo is not null)
        {
            _manageImage.DeletePhoto(petEntity.Photo);
            await _manageImage.UploadPhotoAsync(pet.Photo, petId);

        }
        else
        {
            _logger.LogError($"Photo is null");
            throw new ArgumentException("Photo cannot be null.");
        }

        _mapper.Map(pet, petEntity);


        await _unitOfWorkRep.Pet.UpdateAsync(petEntity);

        await _unitOfWorkRep.SaveAsync();
    }


    public async Task<Pet> CreatePetAsync(Guid userId, PetForCreateDto pet)
    {
        if (userId == Guid.Empty || pet == null)
        {
            _logger.LogError("Error");
            throw new ArgumentNullException("Invalid userId or pet object.");
        }

        //var ownerEntity = await _unitOfWorkRep.Owner.GetAsync(ownerId);
        //var finderEntity = await _unitOfWorkRep.Finder.GetAsync(finderId);
        var userEntity = await _unitOfWorkRep.User.GetAsync(userId);

        var petMap = _mapper.Map<Pet>(pet);
        //petMap.OwnerId = ownerEntity.Id;
        //petMap.FinderId = finderEntity.Id;
        petMap.UserId = userEntity.Id;
        petMap.DateCreateUpdate = DateTime.UtcNow;
        petMap.Photo = await _manageImage.UploadPhotoAsync(pet.Photo, petMap.Id); ;

        await _unitOfWorkRep.Pet.CreateAsync(petMap);

        await _unitOfWorkRep.SaveAsync();

        return petMap;
    }
}