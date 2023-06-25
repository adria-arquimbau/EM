using System.Security.Claims;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EventsManager.Server.Data;
using EventsManager.Server.Handlers.Commands.Events.Create;
using EventsManager.Server.Handlers.Commands.Events.Delete;
using EventsManager.Server.Handlers.Commands.Events.Update;
using EventsManager.Server.Handlers.Queries.Events.Get;
using EventsManager.Server.Handlers.Queries.Events.GetAll;
using EventsManager.Server.Handlers.Queries.Events.GetMyEvent;
using EventsManager.Server.Handlers.Queries.Events.GetMyEvents;
using EventsManager.Server.Models;
using EventsManager.Server.Settings;
using EventsManager.Shared.Dtos;
using EventsManager.Shared.Enums;
using EventsManager.Shared.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventsManager.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class EventController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _context;
    private readonly BlobStorageSettings _blobStorageOptions;

    public EventController(IMediator mediator, ApplicationDbContext context, IOptions<BlobStorageSettings> blobStorageOptions)
    {
        _mediator = mediator;
        _context = context;
        _blobStorageOptions = blobStorageOptions.Value;
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllEvents()
    {
        var response = await _mediator.Send(new GetAllEventsQueryRequest());
        return Ok(response); 
    }
    
    [HttpGet("{eventId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEvent([FromRoute] Guid eventId)
    {
        var response = await _mediator.Send(new GetEventQueryRequest(eventId));
        return Ok(response); 
    }
    
    [HttpGet("{eventId:guid}-organizer")]
    [Authorize(Roles = "Organizer, Staff")] 
    public async Task<IActionResult> GetMyEventAsOrganizer([FromRoute] Guid eventId)
    {   
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var response = await _mediator.Send(new GetMyEventAsOrganizerQueryRequest(eventId, userId));
        return Ok(response); 
    }
    
    [HttpGet("my-events")]
    [Authorize(Roles = "Organizer")] 
    public async Task<IActionResult> GetMyEventsAsOrganizer()
    {   
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var response = await _mediator.Send(new GetMyEventsAsOrganizerQueryRequest(userId));
        return Ok(response); 
    }
    
    [HttpPut("{eventId:guid}/set-owner/{ownerId:guid}")]
    [Authorize(Roles = "Organizer")] 
    public async Task<IActionResult> SetOwner([FromRoute] Guid eventId, [FromRoute] Guid ownerId, CancellationToken cancellationToken)
    {   
        var requesterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;  
        
        var eventToUpdate = await _context.Events
            .Where(x => x.Id == eventId)
            .Include(x => x.Creator)
            .SingleAsync(cancellationToken: cancellationToken);

        if (eventToUpdate.Creator.Id != requesterId)
        {
            return Forbid();
        }
        
        var owner = await _context.Users
            .Where(x => x.Id == ownerId.ToString())
            .SingleAsync(cancellationToken: cancellationToken);
        
        eventToUpdate.Owners.Add(owner);

        var registrationAlreadyExist = await _context.Registrations
            .AnyAsync(x => x.Event.Id == eventToUpdate.Id && x.User.Id == owner.Id && x.Role == RegistrationRole.Staff, cancellationToken: cancellationToken);

        if (!registrationAlreadyExist)
        {
            var registration = new Registration(owner, RegistrationRole.Staff, RegistrationState.Accepted, eventToUpdate);
            registration.CheckedIn = true;
            eventToUpdate.Registrations.Add(registration);
        }
            
        await _context.SaveChangesAsync(cancellationToken);
        return Ok();
    }
    
    [HttpDelete("{eventId:guid}/owner/{ownerId:guid}")]
    [Authorize(Roles = "Organizer")] 
    public async Task<IActionResult> DeleteOwner([FromRoute] Guid eventId, [FromRoute] Guid ownerId, CancellationToken cancellationToken)
    {   
        var requesterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;  
        
        var eventToUpdate = await _context.Events
            .Where(x => x.Id == eventId)
            .Include(x => x.Creator)
            .Include(x => x.Owners)
            .SingleAsync(cancellationToken: cancellationToken);

        if (eventToUpdate.Creator.Id != requesterId)
        {
            return Forbid();
        }

        if (eventToUpdate.Creator.Id == ownerId.ToString())
        {
            return BadRequest("You can't remove the creator of the event as an owner.");
        }
        
        var owner = await _context.Users
            .Where(x => x.Id == ownerId.ToString())
            .SingleAsync(cancellationToken: cancellationToken);

        eventToUpdate.Owners.Remove(owner);

        await _context.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }
    
    [HttpGet("{eventId:guid}/am-i-the-creator")]
    [Authorize(Roles = "Organizer")] 
    public async Task<IActionResult> AmITheCreator([FromRoute] Guid eventId, CancellationToken cancellationToken)
    {   
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var eventToUpdate = await _context.Events
            .Where(x => x.Id == eventId)
            .Include(x => x.Creator)
            .SingleAsync(cancellationToken: cancellationToken);
        
        var amITheCreator = eventToUpdate.Creator.Id == userId;
        
        if (amITheCreator)
        {   
            return Ok();
        }
        
        return Forbid();
    }
    
    [HttpPost]
    [Authorize(Roles = "Organizer")] 
    public async Task<IActionResult> CreateAnEvent([FromBody] CreateEventRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        try
        {
            await _mediator.Send(new CreateEventCommandRequest(request, userId));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
        return Ok(); 
    }
    
    [HttpDelete("{eventId:guid}")]
    [Authorize(Roles = "Organizer")] 
    public async Task<IActionResult> DeleteAnEvent([FromRoute] Guid eventId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await _mediator.Send(new DeleteEventCommandRequest(eventId, userId));
        return Ok(); 
    }   
    
    [HttpPut]
    [Authorize(Roles = "Organizer")] 
    public async Task<IActionResult> UpdateAnEvent([FromBody] MyEventDto eventDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        try
        {
            await _mediator.Send(new UpdateEventCommandRequest(eventDto, userId));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
        return Ok(); 
    }
    
    [HttpPut("update-registration-passwords")]
    [Authorize(Roles = "Organizer")] 
    public async Task<IActionResult> UpdateRegistrationPasswords([FromBody] MyEventDto eventDto, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;  
        
        var eventToUpdate = await _context.Events
            .Where(x => x.Id == eventDto.Id)
            .Include(x => x.Owners)
            .Include(x => x.RegistrationRolePasswords)
            .SingleAsync(cancellationToken: cancellationToken);
        
        if (eventToUpdate.Owners.All(o => o.Id != userId) && eventToUpdate.Creator.Id != userId)
        {
            return Forbid();
        }
        
        var registrationRolePasswords = new List<RegistrationRolePassword>();

        if (!string.IsNullOrWhiteSpace(eventDto.MarshallRegistrationPassword))
        {
            registrationRolePasswords.Add(new RegistrationRolePassword { Role = RegistrationRole.Marshal, Password = eventDto.MarshallRegistrationPassword });
        }
        if (!string.IsNullOrWhiteSpace(eventDto.RiderRegistrationPassword))
        {
            registrationRolePasswords.Add(new RegistrationRolePassword { Role = RegistrationRole.Rider, Password = eventDto.RiderRegistrationPassword });
        }
        if (!string.IsNullOrWhiteSpace(eventDto.StaffRegistrationPassword))
        {
            registrationRolePasswords.Add(new RegistrationRolePassword { Role = RegistrationRole.Staff, Password = eventDto.StaffRegistrationPassword });
        }
        if (!string.IsNullOrWhiteSpace(eventDto.RiderMarshallRegistrationPassword))
        {
            registrationRolePasswords.Add(new RegistrationRolePassword { Role = RegistrationRole.RiderMarshal, Password = eventDto.RiderMarshallRegistrationPassword });
        }

        eventToUpdate.RegistrationRolePasswords.Clear();
        eventToUpdate.RegistrationRolePasswords = registrationRolePasswords;
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return Ok(); 
    }

    [HttpGet("{eventId:guid}/registrations")]
    [Authorize(Roles = "Organizer, Staff")]
    public async Task<IActionResult> GetAllRiderRegistrationsByEventId([FromRoute] Guid eventId, [FromQuery] string? search, [FromQuery] RegistrationRole? role, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var eventRequest = await _context.Events
            .Where(x => x.Id == eventId)
            .Include(x => x.Owners)
            .Include(x => x.Registrations.Where(r => r.Role == RegistrationRole.Staff))
            .SingleAsync(cancellationToken: cancellationToken); 

        if (eventRequest.Owners.All(o => o.Id != userId) && eventRequest.CreatorId != userId)   
        {
            var userRegistration = eventRequest.Registrations.FirstOrDefault(x => x.UserId == userId && x is { Role: RegistrationRole.Staff, State: RegistrationState.Accepted });

            if (userRegistration == null)
            {
                throw new UnauthorizedAccessException();
            }
        }
        
        var valueToSearch = search ?? "";
        var registrationsQuery = _context.Registrations 
            .Where(x => x.Event.Id == eventId && x.User.UserName.Contains(valueToSearch))
            .Select(x => new RegistrationDto
            {
                Id = x.Id,
                CreationDate = x.CreationDate,
                Role = x.Role,  
                State = x.State,
                Bib = x.Bib,
                CheckedIn = x.CheckedIn,
                RegisteredUser = new UserDto
                {
                    Id = x.User.Id,
                    UserName = x.User.UserName,
                    Email = x.User.Email,
                    PostalCode = x.User.PostalCode,
                    Country = x.User.Country,
                    City = x.User.City,
                    Address = x.User.Address,
                    FamilyName = x.User.FamilyName,
                    Name = x.User.Name,
                    PhoneNumber = x.User.PhoneNumber,
                    ImageUrl = x.User.ImageUrl,
                    EmailConfirmed = x.User.EmailConfirmed
                }
            })
            .AsQueryable();

        if (role.HasValue)
        {
            registrationsQuery = registrationsQuery.Where(x => x.Role == role);
        }
    
        var response = await registrationsQuery.ToListAsync(cancellationToken: cancellationToken);
        return Ok(response);
    }
    
    [HttpPost("{eventId:guid}/registration/{registrationRole}/verify-password")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> VerifyRegistrationRolePassword([FromRoute] Guid eventId, [FromRoute] RegistrationRole registrationRole, [FromBody] VerifyRegistrationRolePasswordRequest request, CancellationToken cancellationToken)
    {
        var registrationRolePassword = await _context.RegistrationRolePasswords
            .Where(x => x.Event.Id == eventId && x.Role == registrationRole)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (registrationRolePassword?.Password != request.Password)
        {
            return BadRequest();
        }
        
        return Ok();
    }
    
    [HttpPost("{eventId:guid}/image")]
    [Authorize(Roles = "Organizer")]
    public async Task<ActionResult> UploadEventImage([FromRoute] Guid eventId, IFormFile file, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var eventToUpload = await _context.Events
            .Include(x => x.Owners)
            .SingleAsync(x => x.Id == eventId, cancellationToken);
        
        if (eventToUpload.Owners.All(o => o.Id != userId) && eventToUpload.CreatorId != userId)
        {
            return Forbid();
        }
        var blobClient = new BlobClient(_blobStorageOptions.ConnectionString, _blobStorageOptions.EventImageContainerName, eventToUpload.Id.ToString());
        await blobClient.UploadAsync(file.OpenReadStream(), new BlobHttpHeaders { ContentType = file.ContentType }, conditions: null, cancellationToken: cancellationToken);
        
        eventToUpload.ImageUrl = blobClient.Uri;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok();
    }   
    
    [HttpDelete("{eventId:guid}/image")]   
    [Authorize(Roles = "Organizer")] 
    public async Task<IActionResult> DeleteEventImage([FromRoute] Guid eventId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var eventToDeleteImage = await _context.Events
            .Include(x => x.Owners)
            .SingleAsync(x => x.Id == eventId, cancellationToken);

        if (eventToDeleteImage.Owners.All(o => o.Id != userId) && eventToDeleteImage.CreatorId != userId)
        {
            return Forbid();
        }
        
        var blobClient = new BlobClient(_blobStorageOptions.ConnectionString, _blobStorageOptions.EventImageContainerName, eventToDeleteImage.Id.ToString());
        
        await blobClient.DeleteAsync(cancellationToken: cancellationToken);
            
        eventToDeleteImage.ImageUrl = null;
        
        await _context.SaveChangesAsync(cancellationToken);
        return Ok();
    }
    
    [HttpGet("{eventId:guid}/all-tickets")] 
    [Authorize(Roles = "Organizer")]
    public async Task<ActionResult> GetAllEventTickets([FromRoute] Guid eventId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var canIRetrieveTheTickets = await _context.Events
            .Where(x => x.Id == eventId && (x.CreatorId == userId || x.Owners.Any(o => o.Id == userId)))
            .AnyAsync(cancellationToken);

        if (!canIRetrieveTheTickets)
        {
            return Unauthorized("You can't retrieve the tickets of this event");
        }
        
        var tickets = await _context.Tickets
            .Where(x => x.Registration.Event.Id == eventId)
            .Select(x => new TicketDto
            {
                Id = x.Id,
                Title = x.Title,
                Text = x.Text,
                Solved = x.Solved,
                SolvedBy = x.SolvedBy.UserName,
                CreationDate = x.CreationDate,
                SolvedDate = x.SolvedDate,
                CreatedBy = x.Registration.User.UserName,
                TicketResponses = x.Responses.Select(r => new TicketResponseDto
                {
                    Text = r.Text,
                    ResponseDate = r.ResponseDate,
                    IsAdminResponse = r.IsAdminResponse,
                    RespondedBy = r.RespondedBy.UserName
                }).ToList()
            })
            .ToListAsync(cancellationToken);
        
        return Ok(tickets);
    }   
}