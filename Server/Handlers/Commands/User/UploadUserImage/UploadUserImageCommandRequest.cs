using MediatR;

namespace EventsManager.Server.Handlers.Commands.User.UploadUserImage;

public class UploadUserImageCommandRequest : IRequest
{
    public readonly IFormFile File;
    public readonly string? UserId;

    public UploadUserImageCommandRequest(IFormFile file, string? userId)
    {
        File = file;
        UserId = userId;
    }
}