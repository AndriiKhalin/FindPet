using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Domain.DTOs.AuthDTOs;
using FindPet.Domain.ValueObjects;

namespace FindPet.BusinessLogicLayer.CQRS.Commands.Account;

public record ResetPasswordCommand(ResetPasswordDto ResetPassword) : ICommand<AuthResponse>;