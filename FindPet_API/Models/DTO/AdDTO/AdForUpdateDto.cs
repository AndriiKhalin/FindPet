using Microsoft.AspNetCore.Http;

namespace Models.DTO.AdDTO;

public class AdForUpdateDto
{

    public string Description { get; set; }
    public string Location { get; set; }
    public IFormFile Photo { get; set; }
    public DateTime Date { get; set; }
}