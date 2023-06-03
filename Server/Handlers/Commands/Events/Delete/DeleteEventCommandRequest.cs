using MediatR;

namespace EventsManager.Server.Handlers.Commands.Events.Delete;

public class DeleteEventCommandRequest : IRequest
{
    public readonly Guid EventId;
    public readonly string? UserId;

    public DeleteEventCommandRequest(Guid eventId, string? userId)
    {
        EventId = eventId;
        UserId = userId;
    }
}