﻿namespace FindPet.Domain.DTOs.EntitiesDTOs.OwnerDTO;

public class OwnerDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    //public string? PhoneNumber { get; set; }
    public string? Password { get; set; }

    //public string? Photo { get; set; }
    //public DateTime? DateCreateUpdate { get; set; }

    //public DateTime? BirthDate { get; set; }

    //public DateTime? LostPet { get; set; }
    //public bool? IsPet { get; set; }
}