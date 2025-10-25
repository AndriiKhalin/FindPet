using FindPet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FindPet.DataAccessLayer.Data.DataSeed;

public class UserSeed : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        var users = new List<User>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Andrew",
                Email = "khalin2002@gmail.com",
                Password = "10122002",
                PhoneNumber = "+380737303288",
                Photo = "users/andrew_example.jpg",
                BirthDate = new DateTime(2002, 12, 10)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Vanya",
                Email = "vanya2002@gmail.com",
                Password = "10122002",
                PhoneNumber = "+380737303288",
                Photo = "users/vanya_example.jpg",
                BirthDate = new DateTime(2002, 12, 10)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Vlad",
                Email = "vlad2002@gmail.com",
                Password = "10122002",
                PhoneNumber = "+380737303288",
                Photo = "users/vlad_example.jpg",
                BirthDate = new DateTime(2002, 12, 10)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Dima",
                Email = "dima2002@gmail.com",
                Password = "10122002",
                PhoneNumber = "+380737303288",
                Photo = "users/dima_example.jpg",
                BirthDate = new DateTime(2002, 12, 10)
            }
        };

        builder.HasData(users);
    }
}