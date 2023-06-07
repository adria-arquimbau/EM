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
using EventsManager.Server.Settings;
using EventsManager.Shared.Dtos;
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
    [Authorize(Roles = "Organizer")] 
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
    
    [HttpPost]
    [Authorize(Roles = "Organizer")] 
    public async Task<IActionResult> CreateAnEvent([FromBody] CreateEventRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await _mediator.Send(new CreateEventCommandRequest(request, userId));
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
        await _mediator.Send(new UpdateEventCommandRequest(eventDto, userId));
        return Ok(); 
    }   
    
    [HttpGet("{eventId:guid}/registrations")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> GetAllRegistrationsByEventId([FromRoute] Guid eventId, [FromQuery] string? search, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var eventRegistrations = await _context.Events
            .Where(x => x.Id == eventId)
            .Include(x => x.Owner)
            .SingleAsync(cancellationToken: cancellationToken);

        if (eventRegistrations.Owner.Id != userId)
        {
            throw new UnauthorizedAccessException("You are not the owner of this event");
        }
        var valueToSearch = search ?? "";
        var registrations = await _context.Registrations 
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
            .ToListAsync(cancellationToken: cancellationToken);

        return Ok(registrations);
    }
    
    [HttpPost("{eventId:guid}/image")]
    [Authorize(Roles = "Organizer")]
    public async Task<ActionResult> UploadEventImage([FromRoute] Guid eventId, IFormFile file, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var eventToUpload = await _context.Events
            .Include(x => x.Owner)
            .SingleAsync(x => x.Id == eventId, cancellationToken);
        
        if (eventToUpload.Owner.Id != userId)
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
    [Authorize(Roles = "User")] 
    public async Task<IActionResult> DeleteEventImage([FromRoute] Guid eventId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var eventToDeleteImage = await _context.Events
            .Include(x => x.Owner)
            .SingleAsync(x => x.Id == eventId, cancellationToken);

        if (eventToDeleteImage.Owner.Id != userId)
        {
            return Forbid();
        }
        
        var blobClient = new BlobClient(_blobStorageOptions.ConnectionString, _blobStorageOptions.EventImageContainerName, eventToDeleteImage.Id.ToString());
        
        await blobClient.DeleteAsync(cancellationToken: cancellationToken);
            
        eventToDeleteImage.ImageUrl = null;
        
        await _context.SaveChangesAsync(cancellationToken);
        return Ok();
    }
}