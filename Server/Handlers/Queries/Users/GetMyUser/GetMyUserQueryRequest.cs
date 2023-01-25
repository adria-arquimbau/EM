using EventsManager.Shared.Dtos;
using MediatR;

namespace EventsManager.Server.Handlers.Queries.Users.GetMyUser;

public class GetMyUserQueryRequest : IRequest<UserDto>
{
    public readonly string UserId;

    public GetMyUserQueryRequest(string userId)
    {
        UserId = userId;
    }
}