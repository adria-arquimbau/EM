using EventsManager.Server.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Commands.Events.Delete;

public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommandRequest>
{
    private readonly ApplicationDbContext _context;

    public DeleteEventCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task Handle(DeleteEventCommandRequest request, CancellationToken cancellationToken)
    {
        var eventToDelete = await _context.Events
            .Include(x => x.Owner)
            .SingleAsync(e => e.Id == request.EventId, cancellationToken: cancellationToken);

        if (eventToDelete.Owner.Id != request.UserId)
        {
            throw new Exception("You are not the owner of this event");
        }
        
        _context.Remove(eventToDelete);
        await _context.SaveChangesAsync(cancellationToken);
    }
}