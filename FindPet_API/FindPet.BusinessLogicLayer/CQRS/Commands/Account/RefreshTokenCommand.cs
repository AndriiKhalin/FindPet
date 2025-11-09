using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Domain.ValueObjects;

namespace FindPet.BusinessLogicLayer.CQRS.Commands.Account;

public record RefreshTokenCommand(string RefreshToken, string IpAddress) : ICommand<AuthResponse>;