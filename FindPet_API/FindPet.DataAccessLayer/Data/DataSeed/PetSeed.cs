using FindPet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FindPet.DataAccessLayer.Data.DataSeed;

public class PetSeed : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> builder)
    {
        var pets = new List<Pet>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Type = "Dog",
                Breed = "Labrador",
                Nickname = "Baron",
                Gender = "Male",
                Color = "Black",
                Size = "Middle",
                SpecialMarks = "White spot on chest",
                Photo = "https://example.com/dog.jpg",
                LostDate = new DateTime(2024, 05, 01),
                LostLocation = "st. Sumskaya, 10",
                FoundDate = new DateTime(2024, 10, 12),
                FoundLocation = "st. Petrovskaya, 25",
                Status = "Missing"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Type = "Cat",
                Breed = "British Shorthair",
                Nickname = "Myssa",
                Gender = "Female",
                Color = "Redhead",
                Size = "Small",
                SpecialMarks = "Fluffy tail",
                Photo = "https://example.com/cat.jpg",
                LostDate = new DateTime(2024, 04, 25),
                LostLocation = "st. Petrovskaya, 20",
                FoundDate = new DateTime(2024, 10, 12),
                FoundLocation = "st. Petrovskaya, 25",
                Status = "Missing"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Type = "Rabbit",
                Breed = "American furry sheep",
                Nickname = "Woody",
                Gender = "Female",
                Color = "White",
                Size = "Small",
                SpecialMarks = "Fluffy tail",
                Photo = "https://example.com/cat.jpg",
                LostDate = new DateTime(2024, 04, 25),
                LostLocation = "st. Petrovskaya, 20",
                FoundDate = new DateTime(2024, 10, 12),
                FoundLocation = "st. Petrovskaya, 25",
                Status = "Missing"
            }
        };

        builder.HasData(pets);
    }
}