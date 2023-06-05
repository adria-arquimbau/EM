using EventsManager.Server.Data;
using EventsManager.Server.Models;
using EventsManager.Shared.Dtos;
using EventsManager.Shared.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Queries.Events.Get;

public class GetEventQueryHandler : IRequestHandler<GetEventQueryRequest, EventDto>
{
    private readonly ApplicationDbContext _context;

    public GetEventQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<EventDto> Handle(GetEventQueryRequest request, CancellationToken cancellationToken)
    {
        return await _context.Events
            .Where(x => x.Id == request.EventId)
            .Select(x => new EventDto
            {
                Id = x.Id,
                StartDate = x.StartDate,
                Name = x.Name,
                Description = x.Description,
                Location = x.Location,
                ImageUrl = x.ImageUrl,
                PreRegistrationsCount = x.Registrations.Count(r => r.State == RegistrationState.PreRegistered)
            })
            .SingleAsync(cancellationToken: cancellationToken);
    }
}