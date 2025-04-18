﻿using Microsoft.AspNetCore.Http;

namespace FindPet.Domain.DTOs.EntitiesDTOs.AdDTO;

public class AdForUpdateDto
{

    public string? Description { get; set; }
    public string? Location { get; set; }
    public IFormFile? Photo { get; set; }
}