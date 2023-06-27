using EventsManager.Server.Data;
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
        var eventDto = await _context.Events
            .Where(x => x.Id == request.EventId)
            .Include(x => x.Registrations)
            .Include(x => x.RegistrationRolePasswords)
            .Select(x => new 
            {
                Event = x,
                OrderedPrices = x.Prices.OrderBy(p => p.EndDate).ToList()
            })
            .Select(x => new EventDto
            {
                Id = x.Event.Id,
                StartDate = x.Event.StartDate,
                EndDate = x.Event.FinishDate,
                IsFree = x.Event.IsFree,
                Name = x.Event.Name,
                Description = x.Event.Description,
                Location = x.Event.Location,
                ImageUrl = x.Event.ImageUrl,
                PreAndAcceptedRidersRegistrationsCount = x.Event.Registrations.Count(r => r.Role == RegistrationRole.Rider &&
                    (r.State == RegistrationState.Accepted)),
                RiderMarshallHaveRegistrationRolePassword = x.Event.RegistrationRolePasswords.Any(y => y.Role == RegistrationRole.RiderMarshal),
                RiderHaveRegistrationRolePassword = x.Event.RegistrationRolePasswords.Any(y => y.Role == RegistrationRole.Rider),
                StaffHaveRegistrationRolePassword = x.Event.RegistrationRolePasswords.Any(y => y.Role == RegistrationRole.Staff),
                MarshallHaveRegistrationRolePassword = x.Event.RegistrationRolePasswords.Any(y => y.Role == RegistrationRole.Marshal),
                MaxRegistrations = x.Event.MaxRegistrations,
                OpenRegistrationsDate = x.Event.OpenRegistrationsDate,
                CloseRegistrationsDate = x.Event.CloseRegistrationsDate,
                CurrentPrice = x.OrderedPrices.First(p => p.EndDate >= DateTime.Now).Price,
                Prices = x.OrderedPrices.Select(p => new EventPriceDto
                {
                    Id = p.Id,
                    EndDate = p.EndDate,
                    Price = p.Price
                }).OrderBy(p => p.EndDate).ToList()
            })
            .SingleAsync(cancellationToken: cancellationToken);
        
        var currentPriceIndex = eventDto.Prices.FindIndex(p => DateTime.Now >= eventDto.OpenRegistrationsDate && DateTime.Now <= p.EndDate);

        if (currentPriceIndex != -1)
        {
            eventDto.Prices[currentPriceIndex].IsTheCurrentPrice = true;
        }
        
        return eventDto;
    }
}
