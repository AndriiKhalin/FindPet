namespace Models.Entities;

public abstract class User
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }

    public string? Photo { get; set; }

    public DateTime? BirthDate { get; set; }

    public List<Pet>? Pets { get; set; } = new();
    public List<Ad>? Ads { get; set; } = new();
}