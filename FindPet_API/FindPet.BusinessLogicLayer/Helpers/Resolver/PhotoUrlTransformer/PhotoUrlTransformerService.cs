using FindPet.Domain.DTOs.EntitiesDTOs.AdDTO;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;
using FindPet.Domain.Interfaces.ILoggerService;
using FindPet.Media.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FindPet.BusinessLogicLayer.Helpers.Resolver.PhotoUrlTransformer;

public class PhotoUrlTransformerService(
    IMediaStorageService mediaStorageService,
    ILoggerManager logger,
    IConfiguration configuration) : IPhotoUrlTransformerService
{
    /// <summary>
    ///     Transforms a single DTO with photo path to full URL
    /// </summary>
    public async Task<T?> TransformPhotoUrlAsync<T>(T entity) where T : class
    {
        if (entity == null) return null;

        var photoProperty = entity.GetType().GetProperty("Photo");
        if (photoProperty == null || photoProperty.GetValue(entity) is not string blobName ||
            string.IsNullOrEmpty(blobName))
            return entity;

        try
        {
            var secureUrl = await mediaStorageService.GetFileUrlAsync(blobName, TimeSpan.FromHours(24));

            // Set to default placeholder if file doesn't exist
            if (string.IsNullOrEmpty(secureUrl))
            {
                logger.LogWarn($"Photo not found for entity, using placeholder: {blobName}");
                secureUrl = configuration["DefaultPlaceholderImage"] ?? "/assets/no-image.png";
            }

            photoProperty.SetValue(entity, secureUrl);
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to transform photo URL for {blobName}: {ex.Message}");
            // Set placeholder on error
            photoProperty.SetValue(entity, configuration["DefaultPlaceholderImage"] ?? "/assets/no-image.png");
        }

        return entity;
    }

    /// <summary>
    ///     Transforms a collection of DTOs with photo paths to full URLs
    /// </summary>
    public async Task<IEnumerable<T>> TransformPhotoUrlsAsync<T>(IEnumerable<T> dtos) where T : class
    {
        var dtosList = dtos?.ToList();
        if (dtosList == null || dtosList.Count == 0) return dtosList;
        var tasks = dtosList.Select(TransformPhotoUrlAsync);

        return await Task.WhenAll(tasks);
    }

    /// <summary>
    ///     Specific transformation for PetDto
    /// </summary>
    public async Task<PetDto> TransformPetPhotoAsync(PetDto dto)
    {
        return await TransformPhotoUrlAsync(dto);
    }

    /// <summary>
    ///     Specific transformation for UserDto
    /// </summary>
    public async Task<UserDto> TransformUserPhotoAsync(UserDto dto)
    {
        return await TransformPhotoUrlAsync(dto);
    }

    /// <summary>
    ///     Specific transformation for AdDto
    /// </summary>
    public async Task<AdDto> TransformAdPhotoAsync(AdDto dto)
    {
        return await TransformPhotoUrlAsync(dto);
    }

    /// <summary>
    ///     Transforms collections with parallel processing
    /// </summary>
    public async Task<IEnumerable<PetDto>> TransformPetPhotosAsync(IEnumerable<PetDto> dtos)
    {
        return await TransformPhotoUrlsAsync(dtos);
    }

    public async Task<IEnumerable<UserDto>> TransformUserPhotosAsync(IEnumerable<UserDto> dtos)
    {
        return await TransformPhotoUrlsAsync(dtos);
    }

    public async Task<IEnumerable<AdDto>> TransformAdPhotosAsync(IEnumerable<AdDto> dtos)
    {
        return await TransformPhotoUrlsAsync(dtos);
    }
}