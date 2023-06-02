using EventsManager.Server.Data;
using EventsManager.Shared.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Queries.Users.GetAll;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQueryRequest, List<UserDto>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetAllUsersQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<List<UserDto>> Handle(GetAllUsersQueryRequest request, CancellationToken cancellationToken)
    {
        var users = await _dbContext.Users.Select(x => new UserDto
        {
            Id = x.Id,
            UserName = x.UserName,
            Email = x.Email,
            EmailConfirmed = x.EmailConfirmed,
            ImageUrl = x.ImageUrl,
            IsOrganizer = _dbContext.UserRoles.Any(ur => ur.UserId == x.Id && ur.RoleId == "2"),
            Address = x.Address,
            City = x.City,
            Country = x.Country,
            PhoneNumber = x.PhoneNumber,
            PostalCode = x.PostalCode,
            FamilyName = x.FamilyName,
            Name = x.Name
        })
        .ToListAsync(cancellationToken: cancellationToken);

        return users;
    }
}
