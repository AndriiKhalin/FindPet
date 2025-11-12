using FindPet.Domain.DTOs.EntitiesDTOs.AdDTO;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;

namespace FindPet.BusinessLogicLayer.Helpers.Resolver.PhotoUrlTransformer;

public interface IPhotoUrlTransformerService
{
    Task<T?> TransformPhotoUrlAsync<T>(T entity) where T : class;
    Task<IEnumerable<T>> TransformPhotoUrlsAsync<T>(IEnumerable<T> dtos) where T : class;

    Task<PetDto> TransformPetPhotoAsync(PetDto dto);
    Task<UserDto> TransformUserPhotoAsync(UserDto dto);
    Task<AdDto> TransformAdPhotoAsync(AdDto dto);

    Task<IEnumerable<PetDto>> TransformPetPhotosAsync(IEnumerable<PetDto> dtos);
    Task<IEnumerable<UserDto>> TransformUserPhotosAsync(IEnumerable<UserDto> dtos);
    Task<IEnumerable<AdDto>> TransformAdPhotosAsync(IEnumerable<AdDto> dtos);
}