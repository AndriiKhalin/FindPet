using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Domain.DTOs.AuthDTOs;

namespace FindPet.BusinessLogicLayer.CQRS.Queries.Account;

public record GetUserDetailQuery(string UserId) : IQuery<UserDetailDto>;