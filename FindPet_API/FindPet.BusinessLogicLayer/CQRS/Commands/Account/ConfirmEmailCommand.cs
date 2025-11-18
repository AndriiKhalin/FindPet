using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Domain.ValueObjects;

namespace FindPet.BusinessLogicLayer.CQRS.Commands.Account;

public record ConfirmEmailCommand(string UserId, string Token) : ICommand<AuthResponse>;