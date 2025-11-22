using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Domain.DTOs.AuthDTOs;

namespace FindPet.BusinessLogicLayer.CQRS.Queries.Account;

public record GetActiveSessionsQuery(string UserId, string CurrentToken) : IQuery<IEnumerable<SessionDto>>;