using AutoMapper;
using FindPet.BusinessLogicLayer.Helpers.Resolver.PhotoUrlTransformer;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;

namespace FindPet.BusinessLogicLayer.Extensions;

public static class MappingExtensions
{
    /// <summary>
    ///     Maps entity to DTO and transforms photo URL in one call
    /// </summary>
    public static async Task<TDestination> MapWithPhotoAsync<TSource, TDestination>(
        this IMapper mapper,
        TSource source,
        IPhotoUrlTransformerService photoTransformer)
        where TDestination : class
    {
        var dto = mapper.Map<TDestination>(source);
        return await photoTransformer.TransformPhotoUrlAsync(dto);
    }

    /// <summary>
    ///     Maps collection and transforms photo URLs in parallel
    /// </summary>
    public static async Task<IEnumerable<TDestination>> MapWithPhotosAsync<TSource, TDestination>(
        this IMapper mapper,
        IEnumerable<TSource> sources,
        IPhotoUrlTransformerService photoTransformer)
        where TDestination : class
    {
        var dtos = mapper.Map<IEnumerable<TDestination>>(sources);
        return await photoTransformer.TransformPhotoUrlsAsync(dtos);
    }

    // Specific helper methods for common scenarios
    public static async Task<PetDto> MapPetWithPhotoAsync(
        this IMapper mapper,
        object source,
        IPhotoUrlTransformerService photoTransformer)
    {
        var dto = mapper.Map<PetDto>(source);
        return await photoTransformer.TransformPetPhotoAsync(dto);
    }

    // Specific helper methods for common scenarios
    public static async Task<IEnumerable<PetDto>> MapPetWithPhotosAsync(
        this IMapper mapper,
        IEnumerable<object> source,
        IPhotoUrlTransformerService photoTransformer)
    {
        var dto = mapper.Map<IEnumerable<PetDto>>(source);
        return await photoTransformer.TransformPetPhotosAsync(dto);
    }

    public static async Task<UserDto> MapUserWithPhotoAsync(
        this IMapper mapper,
        object source,
        IPhotoUrlTransformerService photoTransformer)
    {
        var dto = mapper.Map<UserDto>(source);
        return await photoTransformer.TransformUserPhotoAsync(dto);
    }

    public static async Task<IEnumerable<UserDto>> MapUserWithPhotosAsync(
        this IMapper mapper,
        IEnumerable<object> source,
        IPhotoUrlTransformerService photoTransformer)
    {
        var dto = mapper.Map<IEnumerable<UserDto>>(source);
        return await photoTransformer.TransformUserPhotosAsync(dto);
    }
}