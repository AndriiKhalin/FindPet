using AutoMapper;
using Interfaces.IEntityRepository;
using Interfaces.IEntityService;
using Interfaces.IImageService;
using Interfaces.ILoggerService;
using Models.DTO.FinderDTO;
using Models.Entities;
using System.Security.Cryptography.Xml;
using System.Security.Principal;

namespace Services.Service.EntityService;

public class FinderService : IFinderService
{
    private readonly IUnitOfWork _unitOfWorkRep;
    private readonly IMapper _mapper;
    private readonly IManageImage<Finder> _manageImage;
    private readonly ILoggerManager _logger;

    public FinderService(IUnitOfWork unitOfWorkRep, IMapper mapper, IManageImage<Finder> manageImage, ILoggerManager logger)
    {
        _unitOfWorkRep = unitOfWorkRep;
        _mapper = mapper;
        _manageImage = manageImage;
        _logger = logger;
    }

    public IEnumerable<Finder> GetFinders()
    {
        return _unitOfWorkRep.Finder.Gets();
    }

    public async Task<Finder?> GetFinderAsync(Guid finderId)
    {
        if (!await FinderExistsAsync(finderId))
        {
            _logger.LogError($"Finder with id: {finderId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid finder Id");
        }

        return await _unitOfWorkRep.Finder.GetAsync(finderId);
    }

    //public async Task<IEnumerable<Ad>?> GetAdsByFinderAsync(Guid finderId)
    //{
    //    if (!await FinderExistsAsync(finderId))
    //    {
    //        _logger.LogError($"Finder with id: {finderId}, hasn't been found in db.");
    //        throw new ArgumentNullException("Invalid finder Id");
    //    }

    //    return await _unitOfWorkRep.Finder.GetAdsByFinder(finderId);
    //}

    //public async Task<IEnumerable<Pet>?> GetPetsByFinderAsync(Guid finderId)
    //{
    //    if (!await FinderExistsAsync(finderId))
    //    {
    //        _logger.LogError($"Finder with id: {finderId}, hasn't been found in db.");
    //        throw new ArgumentNullException("Invalid finder Id");
    //    }

    //    return await _unitOfWorkRep.Finder.GetPetsByFinder(finderId);
    //}

    public async Task<bool> FinderExistsAsync(Guid finderId)
    {
        return await _unitOfWorkRep.Finder.IsExistAsync(finderId);
    }

    public async Task<bool> FinderExistsAsync(string finderFirstName)
    {
        return await _unitOfWorkRep.Finder.IsExistAsync(finderFirstName);
    }

    public async Task DeleteFinderAsync(Guid finderId)
    {
        if (!await FinderExistsAsync(finderId))
        {
            _logger.LogError($"Finder with id: {finderId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid finder Id");
        }

        var finderEntityForDelete = await GetFinderAsync(finderId);

        _manageImage.DeletePhoto(finderEntityForDelete.Photo);

        await _unitOfWorkRep.Finder.DeleteAsync(finderId);

        await _unitOfWorkRep.SaveAsync();
    }

    public async Task UpdateFinderAsync(Guid finderId, FinderForUpdateDto finder)
    {
        if (finder == null)
        {
            _logger.LogError($"Finder object sent from client is null.");
            throw new ArgumentNullException("Finder is null");
        }

        if (!await FinderExistsAsync(finderId))
        {
            _logger.LogError($"Finder with id: {finderId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid finder Id");
        }

        var finderEntity = await GetFinderAsync(finderId);


        if (finder.Photo is not null)
        {
            _manageImage.DeletePhoto(finderEntity.Photo);
            await _manageImage.UploadPhotoAsync(finder.Photo, finderId);

        }
        //else
        //{
        //    _logger.LogError($"Photo is null");
        //    throw new ArgumentException("Photo cannot be null.");
        //}

        _mapper.Map(finder, finderEntity);


        await _unitOfWorkRep.Finder.UpdateAsync(finderEntity);

        await _unitOfWorkRep.SaveAsync();
    }


    public async Task<Finder> CreateFinderAsync(FinderForCreateDto finder)
    {
        if (finder == null)
        {
            _logger.LogError("Error");
            throw new ArgumentNullException("Invalid  finder object.");
        }

        var finderMap = _mapper.Map<Finder>(finder);

        finderMap.DateCreateUpdate = DateTime.UtcNow;
        finderMap.Photo = await _manageImage.UploadPhotoAsync(finder.Photo, finderMap.Id);


        await _unitOfWorkRep.Finder.CreateAsync(finderMap);

        await _unitOfWorkRep.SaveAsync();

        return finderMap;
    }
}