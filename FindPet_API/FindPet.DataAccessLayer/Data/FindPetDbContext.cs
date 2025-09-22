using FindPet.Domain.Entities;
using FindPet.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FindPet.DataAccessLayer.Data;

public class FindPetDbContext(DbContextOptions<FindPetDbContext> options) : IdentityDbContext<AuthUser>(options)
{
    public DbSet<Pet>? Pets { get; set; } = null!;
    public DbSet<User>? Users { get; set; } = null!;

    public DbSet<Ad>? Ads { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.Entity<User>().UseTpcMappingStrategy();

        //modelBuilder.Entity<User>()
        //    .HasDiscriminator<string>("UserType")
        //    .HasValue<User>("User")
        //    .HasValue<Finder>("Finder")
        //    .HasValue<Owner>("Owner");

        //modelBuilder.Entity<User>().ToTable("Users");
        //modelBuilder.Entity<Finder>().ToTable("Finders");
        //modelBuilder.Entity<Owner>().ToTable("Owners");

        //modelBuilder.Entity<Pet>()
        //    .HasOne(x => x.Finder)
        //    .WithMany(y => y.Pets)
        //    .HasForeignKey(x => x.FinderId)
        //    .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    //public DbSet<Finder>? Finders { get; set; } = null!;
    //public DbSet<Owner>? Owners { get; set; } = null!;
}