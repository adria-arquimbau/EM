using EventsManager.Shared.Requests;
using MediatR;

namespace EventsManager.Server.Handlers.Commands.Events.Create;

public class CreateEventCommandRequest : IRequest
{
    public string UserId;
    public CreateEventRequest Request;

    public CreateEventCommandRequest(CreateEventRequest request, string userId)
    {
        UserId = userId;
        Request = request;
    }
}
