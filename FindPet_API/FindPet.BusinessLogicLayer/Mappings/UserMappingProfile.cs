using AutoMapper;
using FindPet.BusinessLogicLayer.CQRS.Commands.User;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;
using FindPet.Domain.Entities;

namespace FindPet.BusinessLogicLayer.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<User, UserForCreateDto>().ReverseMap();
        CreateMap<User, UserForUpdateDto>().ReverseMap();

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