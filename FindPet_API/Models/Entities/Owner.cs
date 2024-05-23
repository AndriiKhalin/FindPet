using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Entities;

[Table("Owners")]
public class Owner : User
{
    public DateTime? LostPet { get; set; }
    public bool? IsPet { get; set; }

}