using AutoMapper;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;
using FindPet.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace FindPet.BusinessLogicLayer.Helpers.UrlResolver;

public class UserResolver : IValueResolver<User, UserDto, string>
{
    private readonly IConfiguration _configuration;

    public UserResolver(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public string Resolve(User source, UserDto destination, string destMember, ResolutionContext context)
    {
        if (!string.IsNullOrEmpty(source.Photo))
        {
            return _configuration["API_url"] + source.Photo;
        }
        return null;
    }
}