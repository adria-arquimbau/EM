using MediatR;

namespace EventsManager.Server.Handlers.Commands.User.DeleteUserImage;

public class DeleteUserImageCommandRequest : IRequest
{
    public readonly string UserId;

    public DeleteUserImageCommandRequest(string userId)
    {
        UserId = userId;
    }
}