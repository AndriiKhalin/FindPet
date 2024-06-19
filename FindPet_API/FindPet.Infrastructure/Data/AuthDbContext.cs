using FindPet.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FindPet.Infrastructure.Data;

public class AuthDbContext : IdentityDbContext<AuthUser>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {

    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}