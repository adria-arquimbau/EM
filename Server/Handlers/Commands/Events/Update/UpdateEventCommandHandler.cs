using EventsManager.Server.Data;
using EventsManager.Server.Models;
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
            .Include(x => x.Prices)
            .Include(x => x.RegistrationRolePasswords)
            .SingleAsync(e => e.Id == request.EventDto.Id, cancellationToken: cancellationToken);

        if (eventToUpdate.Owners.All(o => o.Id != request.UserId))
        {
            throw new Exception("You are not the owner of this event");
        }
        
        var existingEvent = await _context.Events.SingleOrDefaultAsync(x => x.Name == request.EventDto.Name && x.Id != request.EventDto.Id, cancellationToken);
        if (existingEvent != null)
        {
            throw new Exception("Event with this name already exists");
        }
        
        EnsureAtLeastOnePriceCoversRegistrationPeriod(request, eventToUpdate);
        
        eventToUpdate.Update(request.EventDto.Name, request.EventDto.Description, request.EventDto.Location, request.EventDto.MaxRegistrations, request.EventDto.StartDate.ToUniversalTime(), request.EventDto.FinishDate.ToUniversalTime(), request.EventDto.OpenRegistrationsDate.ToUniversalTime(), request.EventDto.CloseRegistrationsDate.ToUniversalTime(), request.EventDto.IsPublic);
        eventToUpdate.IsFree = request.EventDto.IsFree;
        
        await _context.SaveChangesAsync(cancellationToken);

    }

    private static void EnsureAtLeastOnePriceCoversRegistrationPeriod(UpdateEventCommandRequest request, Event eventToUpdate)
    {
        // Ensure that there is at least one price that covers the entire registration period
        var registrationEnd = request.EventDto.CloseRegistrationsDate;

        var pricesDuringRegistration = eventToUpdate.Prices
            .Any(p => p.EndDate >= registrationEnd);

        if (!pricesDuringRegistration)
        {
            throw new Exception("The updated registration period is not covered by any price, please add a price that covers the entire registration period before updating the event.");
        }
    }
}
