using FindPet.BusinessLogicLayer.CQRS.Common;

namespace FindPet.BusinessLogicLayer.CQRS.Commands.Account;

public record LogoutCommand(string RefreshToken) : ICommand<bool>;