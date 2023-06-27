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
        
        // Initialize a variable to store the index of the current price.
        var currentPriceIndex = -1;

        for (var i = 0; i < eventDto.Prices.Count; i++)
        {
            var price = eventDto.Prices[i];

            // Check if this price's end date is later than now and its start date is earlier than or equal to now.
            bool isWithinDateRange = DateTime.Now >= eventDto.OpenRegistrationsDate && DateTime.Now <= price.EndDate;
    
            // If it is and we haven't found a current price yet, or its end date is closer to now than the currently found price,
            // update the current price.
            if (isWithinDateRange && (currentPriceIndex == -1 || price.EndDate < eventDto.Prices[currentPriceIndex].EndDate))
            {
                // Reset the IsTheCurrentPrice property of the previously found price, if any.
                if (currentPriceIndex != -1)
                {
                    eventDto.Prices[currentPriceIndex].IsTheCurrentPrice = false;
                }

                // Update the index of the current price.
                currentPriceIndex = i;
            }
        }

        // If we've found a current price, set its IsTheCurrentPrice property to true.
        if (currentPriceIndex != -1)
        {
            eventDto.Prices[currentPriceIndex].IsTheCurrentPrice = true;
        }

        
        return eventDto;
    }
}
