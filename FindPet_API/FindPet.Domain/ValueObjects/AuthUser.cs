using Microsoft.AspNetCore.Identity;

namespace FindPet.Domain.ValueObjects;

public class AuthUser : IdentityUser
{
    public string? FullName { get; set; }
}