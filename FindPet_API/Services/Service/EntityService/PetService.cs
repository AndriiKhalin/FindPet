﻿using AutoMapper;
using Interfaces.IEntityRepository;
using Interfaces.IEntityService;
using Interfaces.IImageService;
using Interfaces.ILoggerService;
using Models.DTO.PetDTO;
using Models.Entities;
using System.Security.Cryptography.Xml;

namespace Services.Service.EntityService;
public class PetService : IPetService
{
    private readonly IUnitOfWorkRepository _unitOfWorkRep;
    private readonly IMapper _mapper;
    private readonly IManageImage<Pet> _manageImage;
    private readonly ILoggerManager _logger;

    public PetService(IUnitOfWorkRepository unitOfWorkRep, IMapper mapper, IManageImage<Pet> manageImage, ILoggerManager logger)
    {
        _unitOfWorkRep = unitOfWorkRep;
        _mapper = mapper;
        _manageImage = manageImage;
        _logger = logger;
    }

    public async Task<IEnumerable<Pet>> GetPetsAsync()
    {
        return await _unitOfWorkRep.Pet.GetPetsAsync();
    }

    public async Task<Pet?> GetPetAsync(Guid petId)
    {
        if (!await PetExistsAsync(petId))
        {
            _logger.LogError($"Pet with id: {petId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid pet Id");
        }

        return await _unitOfWorkRep.Pet.GetPetAsync(petId);
    }

    public async Task<IEnumerable<Ad>?> GetAdsByPetAsync(Guid petId)
    {
        if (!await PetExistsAsync(petId))
        {
            _logger.LogError($"Pet with id: {petId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid pet Id");
        }

        return await _unitOfWorkRep.Pet.GetAdsByPet(petId);
    }

    public async Task<Finder> GetFinderByPetAsync(Guid petId)
    {
        if (!await PetExistsAsync(petId))
        {
            _logger.LogError($"Pet with id: {petId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid pet Id");
        }

        return await _unitOfWorkRep.Pet.GetFinderByPet(petId);
    }

    public async Task<Owner> GetOwnerByPetAsync(Guid petId)
    {
        if (!await PetExistsAsync(petId))
        {
            _logger.LogError($"Pet with id: {petId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid pet Id");
        }

        return await _unitOfWorkRep.Pet.GetOwnerByPet(petId);
    }

    public async Task<bool> PetExistsAsync(Guid petId)
    {
        return await _unitOfWorkRep.Pet.PetExistsAsync(petId);
    }

    public async Task<bool> PetExistsAsync(string petName)
    {
        return await _unitOfWorkRep.Pet.PetExistsAsync(petName);
    }

    public async Task DeletePetAsync(Guid petId)
    {
        if (!await PetExistsAsync(petId))
        {
            _logger.LogError($"Pet with id: {petId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid pet Id");
        }

        var petEntityForDelete = await GetPetAsync(petId);

        _manageImage.DeleteFile(petEntityForDelete.Photo);

        await _unitOfWorkRep.Pet.DeletePetAsync(petId);

        await _unitOfWorkRep.Save();
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
            await _manageImage.UploadFileAsync(pet.Photo);
            _manageImage.DeleteFile(petEntity.Photo);
        }
        else
        {
            _logger.LogError($"Photo is null");
            throw new ArgumentException("Photo cannot be null.");
        }

        _mapper.Map(pet, petEntity);

        await _unitOfWorkRep.Pet.UpdatePetAsync(petEntity);

        await _unitOfWorkRep.Save();
    }

    public Task<Pet> CreatePetAsync(Guid ownerId, Guid finderId, PetForCreateDto pet)
    {
        throw new NotImplementedException();
    }

    //public async Task<Pet> CreatePetAsync(Guid ownerId, Guid finderId, PetForCreateDto pet)
    //{
    //    if (ownerId == Guid.Empty || finderId == Guid.Empty || pet == null)
    //    {
    //        _logger.LogError("Error");
    //        throw new ArgumentNullException("Invalid ownerId,finderId or pet object.");
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