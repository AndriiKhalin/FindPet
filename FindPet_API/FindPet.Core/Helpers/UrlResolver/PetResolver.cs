using AutoMapper;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;
using FindPet.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace FindPet_API.Helpers.UrlResolver;

public class PetResolver : IValueResolver<Pet, PetDto, string>
{
    private readonly IConfiguration _configuration;

    public PetResolver(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public string Resolve(Pet source, PetDto destination, string destMember, ResolutionContext context)
    {
        if (!string.IsNullOrEmpty(source.Photo))
        {
            return _configuration["API_url"] + source.Photo;
        }
        return null;
    }
}