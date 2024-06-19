using FindPet.Domain.Entities;
using FindPet.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FindPet.Infrastructure.Data;

public class FindPetDbContext : DbContext
{
    public FindPetDbContext(DbContextOptions<FindPetDbContext> options) : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(b => b.MigrationsAssembly("FindPet.API"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.Entity<User>().UseTpcMappingStrategy();

        modelBuilder.Entity<User>()
            .HasDiscriminator<string>("UserType")
            .HasValue<User>("User")
            .HasValue<Finder>("Finder")
            .HasValue<Owner>("Owner");

        //modelBuilder.Entity<User>().ToTable("Users");
        //modelBuilder.Entity<Finder>().ToTable("Finders");
        //modelBuilder.Entity<Owner>().ToTable("Owners");

        #region SetNullDeleteBehavior

        modelBuilder.Entity<Pet>()
                .HasOne(x => x.Owner)
                .WithMany(y => y.Pets)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Restrict); // Замените DeleteBehavior.Restrict на DeleteBehavior.SetNull

        modelBuilder.Entity<Pet>()
            .HasOne(x => x.Finder)
            .WithMany(y => y.Pets)
            .HasForeignKey(x => x.FinderId)
            .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<Ad>()
            .HasOne(x => x.Pet)
            .WithMany(y => y.Ads)
            .HasForeignKey(x => x.PetId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Ad>()
            .HasOne(x => x.User)
            .WithMany(y => y.Ads)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);
        #endregion 
        base.OnModelCreating(modelBuilder);

    }

    public DbSet<Pet>? Pets { get; set; } = null!;
    public DbSet<User>? Users { get; set; } = null!;

    public DbSet<Ad>? Ads { get; set; } = null!;

    public DbSet<Finder>? Finders { get; set; } = null!;
    public DbSet<Owner>? Owners { get; set; } = null!;
}