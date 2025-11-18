using FindPet.BusinessLogicLayer.CQRS.Common;

namespace FindPet.BusinessLogicLayer.CQRS.Commands.Account;

public record RevokeSessionCommand(string UserId, Guid TokenId) : ICommand<bool>;