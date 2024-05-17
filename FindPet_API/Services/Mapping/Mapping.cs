using AutoMapper;
using Models.DTO.AdDTO;
using Models.DTO.FinderDTO;
using Models.DTO.OwnerDTO;
using Models.DTO.PetDTO;
using Models.DTO.UserDTO;
using Models.Entities;

namespace Services.Mapping;

public class Mapping : Profile
{
    public Mapping()
    {
        CreateMap<IUser, UserDto>().ReverseMap();
        CreateMap<IUser, UserForCreateDto>().ReverseMap();
        CreateMap<IUser, UserForUpdateDto>().ReverseMap();

        CreateMap<Owner, OwnerDto>().ReverseMap();
        CreateMap<Owner, OwnerForCreateDto>().ReverseMap();
        CreateMap<Owner, OwnerForUpdateDto>().ReverseMap();

        CreateMap<Finder, FinderDto>().ReverseMap();
        CreateMap<Finder, FinderForCreateDto>().ReverseMap();
        CreateMap<Finder, FinderForUpdateDto>().ReverseMap();

        CreateMap<Pet, PetDto>().ReverseMap();
        CreateMap<Pet, PetForCreateDto>().ReverseMap();
        CreateMap<Pet, PetForUpdateDto>().ReverseMap();

        CreateMap<Ad, AdDto>().ReverseMap();
        CreateMap<Ad, AdForCreateDto>().ReverseMap();
        CreateMap<Ad, AdForUpdateDto>().ReverseMap();
    }
}