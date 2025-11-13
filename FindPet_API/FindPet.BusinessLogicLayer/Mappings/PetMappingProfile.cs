using AutoMapper;
using FindPet.BusinessLogicLayer.CQRS.Commands.Pet;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;
using FindPet.Domain.Entities;

namespace FindPet.BusinessLogicLayer.Mappings;

public class PetMappingProfile : Profile
{
    public PetMappingProfile()
    {
        CreateMap<Pet, PetDto>()
            .ReverseMap();

        CreateMap<Pet, PetForCreateDto>().ReverseMap();
        CreateMap<Pet, PetForUpdateDto>().ReverseMap();

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
    }
}