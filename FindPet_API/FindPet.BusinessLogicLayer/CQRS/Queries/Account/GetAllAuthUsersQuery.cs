using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Domain.DTOs.AuthDTOs;

namespace FindPet.BusinessLogicLayer.CQRS.Queries.Account;

public record GetAllAuthUsersQuery : IQuery<IEnumerable<UserDetailDto>>;