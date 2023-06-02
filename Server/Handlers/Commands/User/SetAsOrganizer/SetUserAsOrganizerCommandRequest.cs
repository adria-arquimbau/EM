using MediatR;

namespace EventsManager.Server.Handlers.Commands.User.SetAsOrganizer;

public class SetUserAsOrganizerCommandRequest : IRequest
{
    public readonly Guid UserId;

    public SetUserAsOrganizerCommandRequest(Guid userId)
    {
        UserId = userId;
    }
}