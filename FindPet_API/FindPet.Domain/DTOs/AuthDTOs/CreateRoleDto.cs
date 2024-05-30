using System.ComponentModel.DataAnnotations;

namespace FindPet.Domain.DTOs.AuthDTOs;

public class CreateRoleDto
{
    [Required(ErrorMessage = "Role Name is required.")]
    public string RoleName { get; set; } = null!;
}