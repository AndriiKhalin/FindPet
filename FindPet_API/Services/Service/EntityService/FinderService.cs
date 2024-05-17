using AutoMapper;
using Interfaces.IEntityRepository;
using Interfaces.IEntityService;
using Interfaces.IImageService;
using Interfaces.ILoggerService;
using Models.DTO.FinderDTO;
using Models.Entities;
using System.Security.Cryptography.Xml;

namespace Services.Service.EntityService;

public class FinderService : IFinderService
{
    private readonly IUnitOfWorkRepository _unitOfWorkRep;
    private readonly IMapper _mapper;
    private readonly IManageImage<Finder> _manageImage;
    private readonly ILoggerManager _logger;

    public FinderService(IUnitOfWorkRepository unitOfWorkRep, IMapper mapper, IManageImage<Finder> manageImage, ILoggerManager logger)
    {
        _unitOfWorkRep = unitOfWorkRep;
        _mapper = mapper;
        _manageImage = manageImage;
        _logger = logger;
    }

    public async Task<IEnumerable<Finder>> GetFindersAsync()
    {
        return await _unitOfWorkRep.Finder.GetFindersAsync();
    }

    public async Task<Finder?> GetFinderAsync(Guid finderId)
    {
        if (!await FinderExistsAsync(finderId))
        {
            _logger.LogError($"Finder with id: {finderId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid finder Id");
        }

        return await _unitOfWorkRep.Finder.GetFinderAsync(finderId);
    }

    public async Task<IEnumerable<Ad>?> GetAdsByFinderAsync(Guid finderId)
    {
        if (!await FinderExistsAsync(finderId))
        {
            _logger.LogError($"Finder with id: {finderId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid finder Id");
        }

        return await _unitOfWorkRep.Finder.GetAdsByFinder(finderId);
    }

    public async Task<IEnumerable<Pet>?> GetPetsByFinderAsync(Guid finderId)
    {
        if (!await FinderExistsAsync(finderId))
        {
            _logger.LogError($"Finder with id: {finderId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid finder Id");
        }

        return await _unitOfWorkRep.Finder.GetPetsByFinder(finderId);
    }

    public async Task<bool> FinderExistsAsync(Guid finderId)
    {
        return await _unitOfWorkRep.Finder.FinderExistsAsync(finderId);
    }

    public async Task<bool> FinderExistsAsync(string finderFirstName)
    {
        return await _unitOfWorkRep.Finder.FinderExistsAsync(finderFirstName);
    }

    public async Task DeleteFinderAsync(Guid finderId)
    {
        if (!await FinderExistsAsync(finderId))
        {
            _logger.LogError($"Finder with id: {finderId}, hasn't been found in db.");
            throw new ArgumentNullException("Invalid finder Id");
        }

        var finderEntityForDelete = await GetFinderAsync(finderId);

        _manageImage.DeleteFile(finderEntityForDelete.Photo);

        await _unitOfWorkRep.Finder.DeleteFinderAsync(finderId);

        await _unitOfWorkRep.Save();
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
            await _manageImage.UploadFileAsync(finder.Photo);
            _manageImage.DeleteFile(finderEntity.Photo);
        }
        else
        {
            _logger.LogError($"Photo is null");
            throw new ArgumentException("Photo cannot be null.");
        }

        _mapper.Map(finder, finderEntity);

        await _unitOfWorkRep.Finder.UpdateFinderAsync(finderEntity);

        await _unitOfWorkRep.Save();
    }

    public Task<Finder> CreateFinderAsync(FinderForCreateDto finder)
    {
        throw new NotImplementedException();
    }

    //public async Task<Finder> CreateFinderAsync(FinderForCreateDto finder)
    //{
    //    if (finder == null)
    //    {
    //        _logger.LogError("Error");
    //        throw new ArgumentNullException("Invalid  finder object.");
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