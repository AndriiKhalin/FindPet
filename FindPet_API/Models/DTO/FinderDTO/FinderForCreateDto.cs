﻿using Microsoft.AspNetCore.Http;

namespace Models.DTO.FinderDTO;

public class FinderForCreateDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }
    public IFormFile? Photo { get; set; }
}