using EventsManager.Shared.Dtos;
using MediatR;

namespace EventsManager.Server.Handlers.Commands.User.UpdateMyUser;

public class UpdateMyUserCommandRequest : IRequest
{
    public readonly string UserId;
    public readonly UserDto UserDto;

    public UpdateMyUserCommandRequest(string userId, UserDto userDto)
    {
        UserId = userId;
        UserDto = userDto;
    }
}   