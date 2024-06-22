using AutoMapper;
using FindPet.Domain.DTOs.EntitiesDTOs.AdDTO;
//using FindPet.Domain.DTOs.EntitiesDTOs.FinderDTO;
//using FindPet.Domain.DTOs.EntitiesDTOs.OwnerDTO;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;
using FindPet.Domain.Entities;
using FindPet_API.Helpers.UrlResolver;

namespace FindPet.Core.Mappings;

public class Mapping : Profile
{
    public Mapping()
    {
        CreateMap<User, UserDto>().ForMember(d => d.Photo, o => o.MapFrom<UserResolver>()).ReverseMap();
        CreateMap<User, UserForCreateDto>().ReverseMap();
        CreateMap<User, UserForUpdateDto>().ReverseMap();

        //CreateMap<Owner, OwnerDto>().ReverseMap();
        //CreateMap<Owner, OwnerForCreateDto>().ReverseMap();
        //CreateMap<Owner, OwnerForUpdateDto>().ReverseMap();

        //CreateMap<Finder, FinderDto>().ReverseMap();
        //CreateMap<Finder, FinderForCreateDto>().ReverseMap();
        //CreateMap<Finder, FinderForUpdateDto>().ReverseMap();S

        CreateMap<Pet, PetDto>().ReverseMap();
        CreateMap<Pet, PetForCreateDto>().ReverseMap();
        CreateMap<Pet, PetForUpdateDto>().ReverseMap();

        CreateMap<Ad, AdDto>().ReverseMap();
        CreateMap<Ad, AdForCreateDto>().ReverseMap();
        CreateMap<Ad, AdForUpdateDto>().ReverseMap();
    }
}