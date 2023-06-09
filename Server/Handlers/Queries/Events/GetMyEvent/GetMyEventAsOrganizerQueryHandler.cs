using EventsManager.Server.Data;
using EventsManager.Shared.Dtos;
using EventsManager.Shared.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Queries.Events.GetMyEvent;

public class GetMyEventAsOrganizerQueryHandler : IRequestHandler<GetMyEventAsOrganizerQueryRequest, MyEventDto>
{
    private readonly ApplicationDbContext _context;

    public GetMyEventAsOrganizerQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<MyEventDto> Handle(GetMyEventAsOrganizerQueryRequest request, CancellationToken cancellationToken)
    {
        var eventEntity = await _context.Events
            .Include(e => e.Owner)
            .Include(e => e.Registrations)
            .Include(e => e.RegistrationRolePasswords)
            .SingleAsync(e => e.Id == request.EventId, cancellationToken: cancellationToken);

        if (eventEntity.Owner.Id != request.UserId)
        {
            var userRegistration = eventEntity.Registrations.FirstOrDefault(x => x.UserId == request.UserId && x is { Role: RegistrationRole.Staff, State: RegistrationState.Accepted });

            if (userRegistration == null)
            {
                throw new UnauthorizedAccessException();
            }
        }

        return new MyEventDto
        {
            Id = eventEntity.Id,
            CreationDate = eventEntity.CreationDate,
            Name = eventEntity.Name,
            Description = eventEntity.Description,
            Location = eventEntity.Location,
            ImageUrl = eventEntity.ImageUrl,
            MaxRegistrations = eventEntity.MaxRegistrations,
            IsPublic = eventEntity.IsPublic,
            OpenRegistrationsDate = eventEntity.OpenRegistrationsDate,  
            CloseRegistrationsDate = eventEntity.CloseRegistrationsDate,
            StartDate = eventEntity.StartDate,
            FinishDate = eventEntity.FinishDate,
            RidersPreRegistrationsCount = eventEntity.Registrations.Count(r => r is { State: RegistrationState.PreRegistered, Role: RegistrationRole.Rider }),
            RidersAcceptedRegistrationsCount = eventEntity.Registrations.Count(r => r is { State: RegistrationState.Accepted, Role: RegistrationRole.Rider }),
            MarshallAcceptedRegistrationsCount = eventEntity.Registrations.Count(r => r is { State: RegistrationState.Accepted, Role: RegistrationRole.Marshal }),
            RiderMarshallAcceptedRegistrationsCount = eventEntity.Registrations.Count(r => r is { State: RegistrationState.Accepted, Role: RegistrationRole.RiderMarshal }),
            StaffRegistrationPassword = eventEntity.RegistrationRolePasswords.SingleOrDefault(r => r.Role == RegistrationRole.Staff)?.Password,
            RiderRegistrationPassword = eventEntity.RegistrationRolePasswords.SingleOrDefault(r => r.Role == RegistrationRole.Rider)?.Password,
            MarshallRegistrationPassword = eventEntity.RegistrationRolePasswords.SingleOrDefault(r => r.Role == RegistrationRole.Marshal)?.Password,
            RiderMarshallRegistrationPassword = eventEntity.RegistrationRolePasswords.SingleOrDefault(r => r.Role == RegistrationRole.RiderMarshal)?.Password
        };
    }
}
