using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;

namespace FindPet.BusinessLogicLayer.CQRS.Queries.User;

public record GetUserByIdQuery(Guid UserId) : IQuery<UserDto>;