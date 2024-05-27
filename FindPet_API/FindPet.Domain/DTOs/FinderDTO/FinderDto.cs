namespace FindPet.Domain.DTOs.FinderDTO;

public class FinderDto
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }

    public string? Photo { get; set; }

    public DateTime? DateCreateUpdate { get; set; }

    public DateTime? BirthDate { get; set; }

    public DateTime? FindPet { get; set; }
}