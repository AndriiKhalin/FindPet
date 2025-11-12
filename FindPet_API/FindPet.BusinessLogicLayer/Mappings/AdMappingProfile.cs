using AutoMapper;
using FindPet.BusinessLogicLayer.CQRS.Commands.Ad;
using FindPet.Domain.DTOs.EntitiesDTOs.AdDTO;
using FindPet.Domain.Entities;

namespace FindPet.BusinessLogicLayer.Mappings;

public class AdMappingProfile : Profile
{
    public AdMappingProfile()
    {
        CreateMap<Ad, AdDto>().ReverseMap();
        CreateMap<Ad, AdForCreateDto>().ReverseMap();
        CreateMap<Ad, AdForUpdateDto>().ReverseMap();

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
    }
}