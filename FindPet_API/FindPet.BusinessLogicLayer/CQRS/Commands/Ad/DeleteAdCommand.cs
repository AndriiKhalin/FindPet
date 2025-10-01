using FindPet.BusinessLogicLayer.CQRS.Common;
using MediatR;

namespace FindPet.BusinessLogicLayer.CQRS.Commands.Ad;

public record DeleteAdCommand(Guid AdId) : ICommand<Unit>;