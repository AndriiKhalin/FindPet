namespace Models.Entities;

public class Pet
{
    public Guid Id { get; set; }
    public string? Type { get; set; }
    public string? Breed { get; set; }
    public string? Nickname { get; set; }
    public string? Gender { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? SpecialMarks { get; set; }
    public string? Photo { get; set; }
    public DateTime? LostDate { get; set; }
    public string? LostLocation { get; set; }
    public DateTime? FoundDate { get; set; }
    public string? FoundLocation { get; set; }
    public string? Status { get; set; }

    public Guid? OwnerId { get; set; }
    public Owner? Owner { get; set; }

    public Guid? FinderId { get; set; }
    public Finder? Finder { get; set; }
    public List<Ad>? Ads { get; set; } = new();
}