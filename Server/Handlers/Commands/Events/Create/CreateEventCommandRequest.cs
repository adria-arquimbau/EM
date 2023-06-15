using EventsManager.Shared.Requests;
using MediatR;

namespace EventsManager.Server.Handlers.Commands.Events.Create;

public class CreateEventCommandRequest : IRequest
{
    public readonly string UserId;
    public readonly CreateEventRequest Request;

    public CreateEventCommandRequest(CreateEventRequest request, string userId)
    {
        UserId = userId;
        Request = request;
    }
}
