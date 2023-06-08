using EventsManager.Server.Data;
using EventsManager.Server.Models;
using EventsManager.Shared.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Commands.Events.Update;

public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommandRequest>
{
    private readonly ApplicationDbContext _context;

    public UpdateEventCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task Handle(UpdateEventCommandRequest request, CancellationToken cancellationToken)
    {
        var eventToUpdate = await _context.Events
            .Include(x => x.Owner)
            .Include(x => x.RegistrationRolePasswords)
            .SingleAsync(e => e.Id == request.EventDto.Id, cancellationToken: cancellationToken);

        if (eventToUpdate.Owner.Id != request.UserId)
        {
            throw new Exception("You are not the owner of this event");
        }

        eventToUpdate.Update(request.EventDto.Name, request.EventDto.Description, request.EventDto.Location, request.EventDto.MaxRegistrations, request.EventDto.StartDate, request.EventDto.FinishDate, request.EventDto.OpenRegistrationsDate, request.EventDto.CloseRegistrationsDate, request.EventDto.IsPublic);

        var registrationRolePasswords = new List<RegistrationRolePassword>();

        if (!string.IsNullOrWhiteSpace(request.EventDto.MarshallRegistrationPassword))
        {
            registrationRolePasswords.Add(new RegistrationRolePassword { Role = RegistrationRole.Marshal, Password = request.EventDto.MarshallRegistrationPassword });
        }
        if (!string.IsNullOrWhiteSpace(request.EventDto.RiderRegistrationPassword))
        {
            registrationRolePasswords.Add(new RegistrationRolePassword { Role = RegistrationRole.Rider, Password = request.EventDto.RiderRegistrationPassword });
        }
        if (!string.IsNullOrWhiteSpace(request.EventDto.StaffRegistrationPassword))
        {
            registrationRolePasswords.Add(new RegistrationRolePassword { Role = RegistrationRole.Staff, Password = request.EventDto.StaffRegistrationPassword });
        }
        if (!string.IsNullOrWhiteSpace(request.EventDto.RiderMarshallRegistrationPassword))
        {
            registrationRolePasswords.Add(new RegistrationRolePassword { Role = RegistrationRole.RiderMarshal, Password = request.EventDto.RiderMarshallRegistrationPassword });
        }

        eventToUpdate.RegistrationRolePasswords.Clear();
        eventToUpdate.RegistrationRolePasswords = registrationRolePasswords;
        
        await _context.SaveChangesAsync(cancellationToken);

    }
}
