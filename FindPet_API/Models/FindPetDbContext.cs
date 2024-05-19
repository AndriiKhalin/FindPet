using Microsoft.EntityFrameworkCore;
using Models.Entities;

namespace Models;

public class FindPetDbContext : DbContext
{
    public FindPetDbContext(DbContextOptions opt) : base(opt)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().UseTpcMappingStrategy();
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

    }

    public DbSet<Pet>? Pets { get; set; } = null!;
    //public DbSet<User>? Users { get; set; } = null!;

    public DbSet<Ad>? Ads { get; set; } = null!;

    public DbSet<Finder>? Finders { get; set; } = null!;
    public DbSet<Owner>? Owners { get; set; } = null!;
}