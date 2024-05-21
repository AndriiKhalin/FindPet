namespace Models.Entities;

public class Ad : BaseEntity
{
    public Guid Id { get; set; }
    public Guid? PetId { get; set; }
    public Guid? UserId { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? Photo { get; set; }
    public DateTime? Date { get; set; }

    public Pet? Pet { get; set; }
    public User? User { get; set; }
}