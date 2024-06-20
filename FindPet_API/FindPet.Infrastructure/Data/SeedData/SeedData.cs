using FindPet.Domain.Entities;

namespace FindPet.Infrastructure.Data.SeedData;

public class SeedData
{
    public static void SeedDates(FindPetDbContext context)
    {

        //context.Database.EnsureDeleted();

        //context.Database.EnsureCreated();

        if (!context.Finders.Any())
        {
            //User
            Finder andrew = new Finder()
            {
                Id = Guid.NewGuid(),
                Name = "Andrew",
                Email = "khalin2002@gmail.com",
                Password = "10122002",
                //PhoneNumber = "+380737303288",
                //BirthDate = new DateTime(2002, 12, 10)
            };
            Finder vanya = new Finder()
            {
                Id = Guid.NewGuid(),
                Name = "Vanya",
                Email = "vanya2002@gmail.com",
                Password = "10122002",
                //PhoneNumber = "+380737303288",
                //BirthDate = new DateTime(2002, 12, 10)
            };

            context.Finders.AddRange(andrew, vanya);

            Owner vlad = new Owner()
            {
                Id = Guid.NewGuid(),
                Name = "Vlad",
                Email = "vlad2002@gmail.com",
                Password = "10122002",
                //PhoneNumber = "+380737303288",
                //BirthDate = new DateTime(2002, 12, 10)
            };

            Owner dima = new Owner()
            {
                Id = Guid.NewGuid(),
                Name = "Dima",
                Email = "dima2002@gmail.com",
                Password = "10122002",
                //PhoneNumber = "+380737303288",
                //BirthDate = new DateTime(2002, 12, 10)
            };

            context.Owners.AddRange(vlad, dima);

            //Category
            Pet dog = new Pet()
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
            };
            Pet cat = new Pet()
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
            };
            Pet rabbit = new Pet()
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
            };

            context.Pets.AddRange(dog, cat, rabbit);


            Ad dogAd = new Ad()
            {
                Id = Guid.NewGuid(),
                PetId = dog.Id,
                UserId = vanya.Id,
                Description = "I saw a similar dog on the street. Shevchenko",
                Location = "st. Shevchenko, 30",
                Photo = "https://example.com/dog_sighting.jpg",
                DateCreateUpdate = new DateTime(2024, 04, 30)
            };

            Ad rabbitAd = new Ad()
            {
                Id = Guid.NewGuid(),
                PetId = rabbit.Id,
                UserId = andrew.Id,
                Description = "Found a cat in the entrance of house No. 5",
                Location = "Mira St., 5",
                Photo = "https://example.com/cat_sighting.jpg",
                DateCreateUpdate = new DateTime(2024, 05, 02)
            };

            Ad catAd = new Ad()
            {
                Id = Guid.NewGuid(),
                PetId = cat.Id,
                UserId = vlad.Id,
                Description = "Found a cat in the entrance of house No. 5",
                Location = "Mira St., 5",
                Photo = "https://example.com/cat_sighting.jpg",
                DateCreateUpdate = new DateTime(2024, 05, 02)
            };

            context.Ads.AddRange(dogAd, rabbitAd, catAd);
            context.SaveChanges();
        }
    }
}