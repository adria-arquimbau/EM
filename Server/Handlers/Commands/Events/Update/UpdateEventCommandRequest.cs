using EventsManager.Shared.Dtos;
using MediatR;

namespace EventsManager.Server.Handlers.Commands.Events.Update;

public class UpdateEventCommandRequest : IRequest
{
    public readonly MyEventDto EventDto;
    public readonly string? UserId;

    public UpdateEventCommandRequest(MyEventDto eventDto, string? userId)
    {
        EventDto = eventDto;
        UserId = userId;
    }
}