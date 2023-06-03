using EventsManager.Shared.Dtos;
using MediatR;

namespace EventsManager.Server.Handlers.Queries.Events.GetAll;

public class GetAllEventsQueryRequest : IRequest<List<EventDto>>
{   
}