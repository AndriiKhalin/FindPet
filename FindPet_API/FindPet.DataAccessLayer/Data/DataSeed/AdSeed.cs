using FindPet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FindPet.DataAccessLayer.Data.DataSeed;

public class AdSeed : IEntityTypeConfiguration<Ad>
{
    public void Configure(EntityTypeBuilder<Ad> builder)
    {
        var ads = new List<Ad>
        {
            new()
            {
                Id = Guid.NewGuid(),
                //UserId = vanya.Id,
                Description = "I saw a similar dog on the street. Shevchenko",
                Location = "st. Shevchenko, 30",
                Photo = "https://example.com/dog_sighting.jpg",
                DateCreateUpdate = new DateTime(2024, 04, 30)
            },
            new()
            {
                Id = Guid.NewGuid(),
                //UserId = andrew.Id,
                Description = "Found a cat in the entrance of house No. 5",
                Location = "Mira St., 5",
                Photo = "https://example.com/cat_sighting.jpg",
                DateCreateUpdate = new DateTime(2024, 05, 02)
            },
            new()
            {
                Id = Guid.NewGuid(),
                //UserId = vlad.Id,
                Description = "Found a cat in the entrance of house No. 5",
                Location = "Mira St., 5",
                Photo = "https://example.com/cat_sighting.jpg",
                DateCreateUpdate = new DateTime(2024, 05, 02)
            }
        };

        builder.HasData(ads);
    }
}