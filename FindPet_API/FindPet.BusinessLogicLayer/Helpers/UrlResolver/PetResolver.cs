using AutoMapper;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;
using FindPet.Domain.Entities;
using FindPet.Media.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FindPet.BusinessLogicLayer.Helpers.UrlResolver;

public class PetResolver : IValueResolver<Pet, PetDto, string>
{
    private readonly IMediaStorageService _mediaStorageService;

    public PetResolver(IConfiguration configuration, IMediaStorageService mediaStorageService)
    {
        _mediaStorageService = mediaStorageService;
    }

    public string Resolve(Pet source, PetDto destination, string destMember, ResolutionContext context)
    {
        if (!string.IsNullOrEmpty(source.Photo))
            return _mediaStorageService.GetFileUrlAsync(source.Photo).GetAwaiter().GetResult();
        return null;
    }
}