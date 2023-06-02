using MediatR;

namespace EventsManager.Server.Handlers.Commands.User.ConfirmEmail;

public class ConfirmEmailCommandRequest : IRequest
{
    public readonly string UserId;

    public ConfirmEmailCommandRequest(string userId)
    {
        UserId = userId;
    }
}