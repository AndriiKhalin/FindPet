using Microsoft.AspNetCore.Http;

namespace Models.DTO.OwnerDTO;

public class OwnerForCreateDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; }

    public IFormFile Photo { get; set; }

    public DateTime BirthDate { get; set; }
}