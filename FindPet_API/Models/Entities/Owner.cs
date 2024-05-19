using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Entities;

//[Table("Owner")]
public class Owner : User
{
    public DateTime? LostPet { get; set; }
    public bool? FoundPet { get; set; }

}