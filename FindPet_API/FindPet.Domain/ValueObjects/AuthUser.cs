using Microsoft.AspNetCore.Identity;

namespace FindPet.Domain.ValueObjects;

public class AuthUser : IdentityUser
{
    public string? Name { get; set; }

    public DateTime? BirthDate { get; set; }

    public string? Photo { get; set; }
}