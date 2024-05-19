using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Entities;

//[Table("Finder")]
public class Finder : User
{
    public DateTime? FindPet { get; set; }

}