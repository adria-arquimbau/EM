using EventsManager.Shared.Dtos;
using MediatR;

namespace EventsManager.Server.Handlers.Queries.Users.GetAll;

public class GetAllUsersQueryRequest : IRequest<List<UserDto>>
{
    public readonly string? UserId;

    public GetAllUsersQueryRequest(string? userId)
    {
        UserId = userId;
    }
}