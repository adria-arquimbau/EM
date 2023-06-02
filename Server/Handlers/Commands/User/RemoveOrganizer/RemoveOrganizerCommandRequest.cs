using MediatR;

namespace EventsManager.Server.Handlers.Commands.User.RemoveOrganizer;

public class RemoveOrganizerCommandRequest : IRequest
{
    public readonly Guid UserId;

    public RemoveOrganizerCommandRequest(Guid userId)
    {
        UserId = userId;
    }
}