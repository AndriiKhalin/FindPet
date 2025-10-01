using FindPet.BusinessLogicLayer.CQRS.Commands.Image;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.Interfaces.IImageService;
using FindPet.Domain.Enums;
using FindPet.Domain.Exceptions;
using PetClass = FindPet.Domain.Entities.Pet;
using UserClass = FindPet.Domain.Entities.User;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.Image;

public class UploadImageCommandHandler : ICommandHandler<UploadImageCommand, UploadImageResponse>
{
    private readonly IManageImage<PetClass> _petImageManager;
    private readonly IManageImage<UserClass> _userImageManager;

    public UploadImageCommandHandler(
        IManageImage<UserClass> userImageManager,
        IManageImage<PetClass> petImageManager)
    {
        _userImageManager = userImageManager;
        _petImageManager = petImageManager;
    }

    public async Task<UploadImageResponse> Handle(UploadImageCommand request, CancellationToken cancellationToken)
    {
        var uniqueId = Guid.NewGuid();
        string filePath;

        switch (request.EntityType)
        {
            case EntityType.User:
                filePath = await _userImageManager.UploadPhotoAsync(request.ImageFile, uniqueId);
                break;

            case EntityType.Pet:
                filePath = await _petImageManager.UploadPhotoAsync(request.ImageFile, uniqueId);
                break;

            default:
                throw new BadRequestException($"Unsupported entity type: {request.EntityType}");
        }

        return new UploadImageResponse
        {
            FilePath = filePath,
            FileName = request.ImageFile.FileName
        };
    }
}