using EventsManager.Shared.Dtos;
using MediatR;

namespace EventsManager.Server.Handlers.Queries.Events.GetMyEvent;

public class GetMyEventAsOrganizerQueryRequest : IRequest<MyEventDto>
{
    public readonly Guid EventId;
    public readonly string? UserId;

    public GetMyEventAsOrganizerQueryRequest(Guid eventId, string? userId)
    {
        EventId = eventId;
        UserId = userId;
    }
}