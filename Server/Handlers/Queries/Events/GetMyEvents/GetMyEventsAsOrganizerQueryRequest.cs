using EventsManager.Shared.Dtos;
using MediatR;

namespace EventsManager.Server.Handlers.Queries.Events.GetMyEvents;

public class GetMyEventsAsOrganizerQueryRequest : IRequest<List<MyEventDto>>
{
    public readonly string UserId;

    public GetMyEventsAsOrganizerQueryRequest(string userId)
    {
        UserId = userId;
    }
}