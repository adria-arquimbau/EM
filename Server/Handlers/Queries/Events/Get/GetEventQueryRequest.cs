using EventsManager.Shared.Dtos;
using MediatR;

namespace EventsManager.Server.Handlers.Queries.Events.Get;

public class GetEventQueryRequest : IRequest<EventDto>
{
    public readonly Guid EventId;

    public GetEventQueryRequest(Guid eventId)
    {
        EventId = eventId;
    }
}