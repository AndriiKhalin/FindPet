using AutoMapper;
using FindPet.BusinessLogicLayer.CQRS.Commands.Ad;
using FindPet.BusinessLogicLayer.CQRS.Commands.Pet;
using FindPet.BusinessLogicLayer.CQRS.Commands.User;
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
            .ForPath(dest => dest.Nickname, opt => opt.MapFrom(src => src.Pet.Nickname))
            .ForPath(dest => dest.Breed, opt => opt.MapFrom(src => src.Pet.Breed))
            .ForPath(dest => dest.Color, opt => opt.MapFrom(src => src.Pet.Color))
            .ForPath(dest => dest.Size, opt => opt.MapFrom(src => src.Pet.Size))
            .ForPath(dest => dest.Gender, opt => opt.MapFrom(src => src.Pet.Gender))
            .ForPath(dest => dest.Description, opt => opt.MapFrom(src => src.Pet.Description))
            .ForPath(dest => dest.LostDate, opt => opt.MapFrom(src => src.Pet.LostDate))
            .ForPath(dest => dest.Photo, opt => opt.MapFrom(src => src.Pet.Photo));

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

        CreateMap<CreateAdCommand, Ad>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.PetId, opt => opt.MapFrom(src => src.PetId))
            .ForMember(dest => dest.DateCreateUpdate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForPath(dest => dest.Description, opt => opt.MapFrom(src => src.Ad.Description))
            .ForPath(dest => dest.Location, opt => opt.MapFrom(src => src.Ad.Location))
            .ForPath(dest => dest.Photo, opt => opt.MapFrom(src => src.Ad.Photo));

        CreateMap<UpdateAdCommand, Ad>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AdId))
            .ForMember(dest => dest.DateCreateUpdate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForPath(dest => dest.Description, opt => opt.MapFrom(src => src.Ad.Description))
            .ForPath(dest => dest.Location, opt => opt.MapFrom(src => src.Ad.Location))
            .ForPath(dest => dest.Photo, opt => opt.MapFrom(src => src.Ad.Photo));

        CreateMap<CreateUserCommand, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.User.Name))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.User.Password))
            .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.User.BirthDate))
            .ForMember(dest => dest.Photo, opt => opt.MapFrom(src => src.User.Photo))
            .ForMember(dest => dest.DateCreateUpdate, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<UpdateUserCommand, User>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.User.Name))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.User.Password))
            .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.User.BirthDate))
            .ForMember(dest => dest.Photo, opt => opt.MapFrom(src => src.User.Photo))
            .ForMember(dest => dest.DateCreateUpdate, opt => opt.MapFrom(src => DateTime.UtcNow));
    }
}