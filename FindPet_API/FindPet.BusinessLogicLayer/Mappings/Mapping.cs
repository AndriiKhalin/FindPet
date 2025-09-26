using AutoMapper;
using FindPet.BusinessLogicLayer.CQRS.Commands.Pet;
using FindPet.BusinessLogicLayer.Helpers.UrlResolver;
using FindPet.Domain.DTOs.EntitiesDTOs.AdDTO;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;
using FindPet.Domain.Entities;

//using FindPet.Domain.DTOs.EntitiesDTOs.FinderDTO;
//using FindPet.Domain.DTOs.EntitiesDTOs.OwnerDTO;

namespace FindPet.BusinessLogicLayer.Mappings;

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
        //CreateMap<Finder, FinderForUpdateDto>().ReverseMap();

        CreateMap<Pet, PetDto>().ForMember(d => d.Photo, o => o.MapFrom<PetResolver>()).ReverseMap();
        CreateMap<Pet, PetForCreateDto>().ReverseMap();
        CreateMap<Pet, PetForUpdateDto>().ReverseMap();

        CreateMap<Ad, AdDto>().ReverseMap();
        CreateMap<Ad, AdForCreateDto>().ReverseMap();
        CreateMap<Ad, AdForUpdateDto>().ReverseMap();

        CreateMap<CreatePetCommand, PetDto>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DateCreateUpdate, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<UpdatePetCommand, PetForUpdateDto>()
            .ForMember(dest => dest, opt => opt.Ignore()); // Don't update UserId

        // Entity to DTO mappings
        CreateMap<Pet, PetDto>()
            .ReverseMap();
    }
}