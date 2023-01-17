using EventsManager.Server.Data;
using EventsManager.Shared.Dtos;
using EventsManager.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Queries;

public class GetMyUserCommandHandler : IRequestHandler<GetMyUserCommandRequest, UserDto>
{
    private readonly ApplicationDbContext _dbContext;

    public GetMyUserCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task<UserDto> Handle(GetMyUserCommandRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .SingleAsync(x => x.Id == request.UserId, cancellationToken);

        if(user == null)
        {
            throw new UserNotFoundException("User not found with Id: " + request.UserId);
        }
        
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            Name = user.Name,
            FamilyName = user.FamilyName,
            Address = user.Address,
            City = user.City,
            Country = user.Country,
            PostalCode = user.PostalCode,
            PhoneNumber = user.PhoneNumber
        };
    }
}
