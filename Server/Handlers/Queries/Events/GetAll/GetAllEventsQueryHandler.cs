using EventsManager.Server.Data;
using EventsManager.Shared.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Queries.Events.GetAll;

public class GetAllEventsQueryHandler : IRequestHandler<GetAllEventsQueryRequest, List<EventDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllEventsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<EventDto>> Handle(GetAllEventsQueryRequest request, CancellationToken cancellationToken)
    {
        return await _context.Events
            .Where(x => x.IsPublic)
            .Select(x => new EventDto
            {
                Id = x.Id,
                StartDate = x.StartDate,    
                Name = x.Name,
                Description = x.Description,
                Location = x.Location,
                ImageUrl = x.ImageUrl
            }).ToListAsync(cancellationToken: cancellationToken);
    }
}