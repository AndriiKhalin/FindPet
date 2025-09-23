using FindPet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FindPet.DataAccessLayer.Data.Configurations;

public class AdConfiguration : IEntityTypeConfiguration<Ad>
{
    public void Configure(EntityTypeBuilder<Ad> builder)
    {
        builder
            .HasOne(x => x.Pet)
            .WithMany(y => y.Ads)
            .HasForeignKey(x => x.PetId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.User)
            .WithMany(y => y.Ads)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}