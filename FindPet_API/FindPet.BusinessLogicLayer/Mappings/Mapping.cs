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

        CreateMap<CreatePetCommand, Pet>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.DateCreateUpdate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForPath(dest => dest.Nickname, opt => opt.MapFrom(src => src.PetCreate.Nickname))
            .ForPath(dest => dest.Breed, opt => opt.MapFrom(src => src.PetCreate.Breed))
            .ForPath(dest => dest.Color, opt => opt.MapFrom(src => src.PetCreate.Color))
            .ForPath(dest => dest.Size, opt => opt.MapFrom(src => src.PetCreate.Size))
            .ForPath(dest => dest.Gender, opt => opt.MapFrom(src => src.PetCreate.Gender))
            .ForPath(dest => dest.Description, opt => opt.MapFrom(src => src.PetCreate.Description))
            .ForPath(dest => dest.LostDate, opt => opt.MapFrom(src => src.PetCreate.LostDate))
            .ForPath(dest => dest.Photo, opt => opt.MapFrom(src => src.PetCreate.Photo));

        CreateMap<UpdatePetCommand, Pet>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PetId))
            .ForMember(dest => dest.DateCreateUpdate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForPath(dest => dest.Nickname, opt => opt.MapFrom(src => src.PetUpdate.Nickname))
            .ForPath(dest => dest.Breed, opt => opt.MapFrom(src => src.PetUpdate.Breed))
            .ForPath(dest => dest.Color, opt => opt.MapFrom(src => src.PetUpdate.Color))
            .ForPath(dest => dest.Size, opt => opt.MapFrom(src => src.PetUpdate.Size))
            .ForPath(dest => dest.Gender, opt => opt.MapFrom(src => src.PetUpdate.Gender))
            .ForPath(dest => dest.Description, opt => opt.MapFrom(src => src.PetUpdate.Description))
            .ForPath(dest => dest.LostDate, opt => opt.MapFrom(src => src.PetUpdate.LostDate))
            .ForPath(dest => dest.Photo, opt => opt.MapFrom(src => src.PetUpdate.Photo));

        // Entity to DTO mappings
        CreateMap<Pet, PetDto>()
            .ReverseMap();
    }
}