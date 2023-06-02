using EventsManager.Server.Data;
using EventsManager.Server.Models;
using EventsManager.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Commands.User.SetAsOrganizer;

public class SetUserAsOrganizerCommandHandler : IRequestHandler<SetUserAsOrganizerCommandRequest>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;

    public SetUserAsOrganizerCommandHandler(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task Handle(SetUserAsOrganizerCommandRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .SingleAsync(x => x.Id == request.UserId.ToString(), cancellationToken: cancellationToken);

        await _userManager.AddToRoleAsync(user, RoleConstants.Organizer);
    }
}