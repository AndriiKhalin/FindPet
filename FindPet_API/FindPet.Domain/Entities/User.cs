namespace FindPet.Domain.Entities;

public class User : BaseEntity
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }

    public string? Photo { get; set; }

    public DateTime? BirthDate { get; set; }

    public DateTime? DateCreateUpdate { get; set; }
    public List<Pet>? Pets { get; set; } = new();
    public List<Ad>? Ads { get; set; } = new();
}