using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Domain.DTOs.AuthDTOs;

namespace FindPet.BusinessLogicLayer.CQRS.Queries.Role;

public record GetAllRolesQuery : IQuery<IEnumerable<RoleResponseDto>>;