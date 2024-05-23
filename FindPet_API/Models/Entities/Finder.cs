using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Entities;

[Table("Finders")]
public class Finder : User
{
    public DateTime? FindPet { get; set; }

}