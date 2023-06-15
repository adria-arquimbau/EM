using EventsManager.Server.Data;
using EventsManager.Server.Models;
using EventsManager.Shared.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Commands.Events.Create;

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommandRequest>
{
    private readonly ApplicationDbContext _context;

    public CreateEventCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task Handle(CreateEventCommandRequest request, CancellationToken cancellationToken)
    {
        var owner = await _context.Users.SingleAsync(x => x.Id == request.UserId, cancellationToken);

        var newEvent = new Event(request.Request.Name, request.Request.Description, request.Request.Location, request.Request.MaxRegistrations, owner, request.Request.StartDate.ToUniversalTime(), request.Request.FinishDate.ToUniversalTime(), request.Request.OpenRegistrationsDate.ToUniversalTime(), request.Request.CloseRegistrationsDate.ToUniversalTime());
        
        newEvent.Registrations.Add(new Registration(owner, RegistrationRole.Staff, RegistrationState.Accepted, newEvent));
        newEvent.IsFree = request.Request.IsFree;
        
        newEvent.Prices.Add(new EventPrice(request.Request.FirstPrice == 0 ? 1 : request.Request.FirstPrice,  request.Request.StartDate.ToUniversalTime(),  request.Request.FinishDate.ToUniversalTime()));
        
        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
    