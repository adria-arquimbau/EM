using EventsManager.Server.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Commands.Events.Update;

public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommandRequest>
{
    private readonly ApplicationDbContext _context;

    public UpdateEventCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task Handle(UpdateEventCommandRequest request, CancellationToken cancellationToken)
    {
        var eventToUpdate = await _context.Events
            .Include(x => x.Owners)
            .Include(x => x.RegistrationRolePasswords)
            .SingleAsync(e => e.Id == request.EventDto.Id, cancellationToken: cancellationToken);

        if (eventToUpdate.Owners.All(o => o.Id != request.UserId))
        {
            throw new Exception("You are not the owner of this event");
        }

        eventToUpdate.Update(request.EventDto.Name, request.EventDto.Description, request.EventDto.Location, request.EventDto.MaxRegistrations, request.EventDto.StartDate, request.EventDto.FinishDate, request.EventDto.OpenRegistrationsDate, request.EventDto.CloseRegistrationsDate, request.EventDto.IsPublic);
        eventToUpdate.IsFree = request.EventDto.IsFree;
        await _context.SaveChangesAsync(cancellationToken);

    }
}
