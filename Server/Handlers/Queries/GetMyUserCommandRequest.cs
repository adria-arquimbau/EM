using EventsManager.Shared;
using EventsManager.Shared.Dtos;
using MediatR;

namespace EventsManager.Server.Handlers.Queries;

public class GetMyUserCommandRequest : IRequest<UserDto>
{
    public readonly string UserId;

    public GetMyUserCommandRequest(string userId)
    {
        UserId = userId;
    }
}