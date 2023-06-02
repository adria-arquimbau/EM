using EventsManager.Server.Data;
using EventsManager.Server.Models;
using EventsManager.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Commands.User.RemoveOrganizer;

public class RemoveOrganizerCommandHandler : IRequestHandler<RemoveOrganizerCommandRequest>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;

    public RemoveOrganizerCommandHandler(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }
    public async Task Handle(RemoveOrganizerCommandRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .SingleAsync(x => x.Id == request.UserId.ToString(), cancellationToken: cancellationToken);

        await _userManager.RemoveFromRoleAsync(user, RoleConstants.Organizer);
    }
}
