using EventsManager.Server.Data;
using EventsManager.Shared.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Queries.Events.GetMyEvents;

public class GetMyEventsAsOrganizerQueryHandler : IRequestHandler<GetMyEventsAsOrganizerQueryRequest, List<MyEventDto>>
{
    private readonly ApplicationDbContext _context;

    public GetMyEventsAsOrganizerQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<MyEventDto>> Handle(GetMyEventsAsOrganizerQueryRequest request, CancellationToken cancellationToken)
    {
        var events = await _context.Users
            .Where(x => x.Id == request.UserId)
            .SelectMany(x => x.OwnedEvents.Select(e => new { Event = e, IsOwned = true }))
            .Union(_context.Users
                .Where(x => x.Id == request.UserId)
                .SelectMany(x => x.CreatorEvents.Select(e => new { Event = e, IsOwned = false })))
            .GroupBy(x => x.Event.Id)
            .Select(g => new MyEventDto
            {
                Id = g.Key,
                CreationDate = g.First().Event.CreationDate,
                StartDate = g.First().Event.StartDate,
                FinishDate = g.First().Event.FinishDate,
                Name = g.First().Event.Name,
                Description = g.First().Event.Description,
                Location = g.First().Event.Location,
                ImageUrl = g.First().Event.ImageUrl,
                MaxRegistrations = g.First().Event.MaxRegistrations,
                IsPublic = g.First().Event.IsPublic,
                OpenRegistrationsDate = g.First().Event.OpenRegistrationsDate,
                CloseRegistrationsDate = g.First().Event.CloseRegistrationsDate
            })
            .ToListAsync(cancellationToken: cancellationToken);

        return events;

    }
}