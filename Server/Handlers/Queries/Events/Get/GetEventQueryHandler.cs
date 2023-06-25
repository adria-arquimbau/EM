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
        return await _context.Events
            .Where(x => x.Id == request.EventId)
            .Include(x => x.Registrations)
            .Include(x => x.RegistrationRolePasswords)
            .Select(x => new EventDto
            {
                Id = x.Id,
                StartDate = x.StartDate,
                EndDate = x.FinishDate,
                IsFree = x.IsFree,
                Name = x.Name,
                Description = x.Description,
                Location = x.Location,
                ImageUrl = x.ImageUrl,
                PreAndAcceptedRidersRegistrationsCount = x.Registrations.Count(r => r.Role == RegistrationRole.Rider &&
                    (r.State == RegistrationState.Accepted)),
                RiderMarshallHaveRegistrationRolePassword = x.RegistrationRolePasswords.Any(y => y.Role == RegistrationRole.RiderMarshal),
                RiderHaveRegistrationRolePassword = x.RegistrationRolePasswords.Any(y => y.Role == RegistrationRole.Rider),
                StaffHaveRegistrationRolePassword = x.RegistrationRolePasswords.Any(y => y.Role == RegistrationRole.Staff),
                MarshallHaveRegistrationRolePassword = x.RegistrationRolePasswords.Any(y => y.Role == RegistrationRole.Marshal),
                MaxRegistrations = x.MaxRegistrations,
                OpenRegistrationsDate = x.OpenRegistrationsDate,
                CloseRegistrationsDate = x.CloseRegistrationsDate,
                CurrentPrice = x.Prices.FirstOrDefault(p => p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now).Price,
            })
            .SingleAsync(cancellationToken: cancellationToken);
    }
}