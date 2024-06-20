namespace FindPet.Domain.Entities;

//[Table("Owners")]
public class Owner : User
{

    public DateTime? LostPet { get; set; }
    public bool? IsPet { get; set; }

    public List<Ad>? Ads { get; set; } = new();

}