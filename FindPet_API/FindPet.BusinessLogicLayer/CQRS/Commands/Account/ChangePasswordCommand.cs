using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Domain.DTOs.AuthDTOs;
using FindPet.Domain.ValueObjects;

namespace FindPet.BusinessLogicLayer.CQRS.Commands.Account;

public record ChangePasswordCommand(string UserId, ChangePasswordDto ChangePassword) : ICommand<AuthResponse>;