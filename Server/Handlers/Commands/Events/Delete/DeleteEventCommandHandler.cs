using EventsManager.Server.Data;
using EventsManager.Server.Models;
using EventsManager.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Commands.Events.Delete;

public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommandRequest>
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteEventCommandHandler(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    
    public async Task Handle(DeleteEventCommandRequest request, CancellationToken cancellationToken)
    {
        var eventToDelete = await _context.Events
            .Include(x => x.Owners)
            .Include(x => x.Registrations.Where(r => r.State == RegistrationState.Accepted && r.Role == RegistrationRole.Staff))
            .ThenInclude(x => x.User)
            .SingleAsync(e => e.Id == request.EventId, cancellationToken: cancellationToken);

        if (eventToDelete.Owners.All(o => o.Id != request.UserId))
        {
            throw new Exception("You are not the owner of this event");
        }

        var acceptedStaffRegistrations = eventToDelete.Registrations;
        
        foreach (var registration in acceptedStaffRegistrations)
        {
            var otherAcceptedStaffRegistrationCount = await _context.Registrations
                .CountAsync(x =>
                    x.User.Id == registration.UserId && x.Event.Id != eventToDelete.Id &&
                    x.State == RegistrationState.Accepted && x.Role == RegistrationRole.Staff, cancellationToken: cancellationToken);

            if (otherAcceptedStaffRegistrationCount == 0)
            {
                var user = await _userManager.FindByIdAsync(registration.UserId);
                await _userManager.RemoveFromRoleAsync(user, "Staff");
            }
        }

        eventToDelete.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
    }
}