﻿namespace FindPet.Domain.DTOs.AuthDTOs;

public class RoleResponseDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public int TotalUsers { get; set; }
}