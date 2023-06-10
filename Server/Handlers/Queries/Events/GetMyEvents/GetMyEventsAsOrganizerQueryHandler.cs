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
        return await _context.Events
            .Where(x => x.Owners.All(o => o.Id == request.UserId))
            .Select(x => new MyEventDto
            {
                Id = x.Id,
                CreationDate = x.CreationDate,
                StartDate = x.StartDate,
                FinishDate = x.FinishDate,
                Name = x.Name,
                Description = x.Description,
                Location = x.Location,
                ImageUrl = x.ImageUrl,
                MaxRegistrations = x.MaxRegistrations,
                IsPublic = x.IsPublic,
                OpenRegistrationsDate = x.OpenRegistrationsDate,
                CloseRegistrationsDate = x.CloseRegistrationsDate,
            }).ToListAsync(cancellationToken: cancellationToken);
    }
}