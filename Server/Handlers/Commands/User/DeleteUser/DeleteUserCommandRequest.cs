using MediatR;

namespace EventsManager.Server.Handlers.Commands.User.DeleteUser;

public class DeleteUserCommandRequest : IRequest
{
    public readonly string UserId;

    public DeleteUserCommandRequest(string userId)
    {
        UserId = userId;
    }
}