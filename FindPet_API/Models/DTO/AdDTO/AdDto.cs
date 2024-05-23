namespace Models.DTO.AdDTO;

public class AdDto
{
    public Guid Id { get; set; }
    public Guid? PetId { get; set; }
    public Guid? UserId { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? Photo { get; set; }
    public DateTime? DateCreateUpdate { get; set; }
}