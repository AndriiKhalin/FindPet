namespace Models.Entities;

public class Finder : IUser
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; }
    public string Photo { get; set; }
    public DateTime BirthDate { get; set; }
    public DateTime FindPet { get; set; }
    public List<Pet>? Pets { get; set; }
    public List<Ad>? Ads { get; set; }
}