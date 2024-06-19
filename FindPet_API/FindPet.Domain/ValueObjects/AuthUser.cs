using Microsoft.AspNetCore.Identity;

namespace FindPet.Domain.ValueObjects;

public class AuthUser : IdentityUser
{
    public string? Name { get; set; }
}