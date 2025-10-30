using AutoMapper;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;
using FindPet.Domain.Entities;
using FindPet.Media.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FindPet.BusinessLogicLayer.Helpers.UrlResolver;

public class UserResolver : IValueResolver<User, UserDto, string>
{
    private readonly IMediaStorageService _mediaStorageService;

    public UserResolver(IConfiguration configuration, IMediaStorageService mediaStorageService)
    {
        _mediaStorageService = mediaStorageService;
    }

    public string Resolve(User source, UserDto destination, string destMember, ResolutionContext context)
    {
        if (!string.IsNullOrEmpty(source.Photo))
            return _mediaStorageService.GetFileUrlAsync(source.Photo).GetAwaiter().GetResult();
        return null;
    }
}