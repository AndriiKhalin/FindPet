using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FindPet.Domain.ValueObjects;

public class AuthUser : IdentityUser
{
    public string? Name { get; set; }

    public DateTime? BirthDate { get; set; }

    public string? Photo { get; set; }
}