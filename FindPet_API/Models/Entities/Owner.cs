namespace Models.Entities;

public class Owner : IUser
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; }
    public string Photo { get; set; }
    public DateTime LostPet { get; set; }
    public bool FoundPet { get; set; }
    public DateTime BirthDate { get; set; }
    public List<Pet>? Pets { get; set; }
    public List<Ad>? Ads { get; set; }
}