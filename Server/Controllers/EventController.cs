using System.Security.Claims;
using EventsManager.Server.Data;
using EventsManager.Server.Handlers.Commands.Events.Create;
using EventsManager.Server.Handlers.Commands.Events.Delete;
using EventsManager.Server.Handlers.Commands.Events.Update;
using EventsManager.Server.Handlers.Queries.Events.Get;
using EventsManager.Server.Handlers.Queries.Events.GetAll;
using EventsManager.Server.Handlers.Queries.Events.GetMyEvent;
using EventsManager.Server.Handlers.Queries.Events.GetMyEvents;
using EventsManager.Shared.Dtos;
using EventsManager.Shared.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class EventController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _context;

    public EventController(IMediator mediator, ApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
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
    public async Task<IActionResult> GetAllByEventId([FromRoute] Guid eventId, [FromQuery] string? search, CancellationToken cancellationToken)
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
            .Where(x => x.EventId == eventId && x.RegisteredUser.UserName.Contains(valueToSearch))
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
                    Id = x.RegisteredUser.Id,
                    UserName = x.RegisteredUser.UserName,
                    Email = x.RegisteredUser.Email,
                    PostalCode = x.RegisteredUser.PostalCode,
                    Country = x.RegisteredUser.Country,
                    City = x.RegisteredUser.City,
                    Address = x.RegisteredUser.Address,
                    FamilyName = x.RegisteredUser.FamilyName,
                    Name = x.RegisteredUser.Name,
                    PhoneNumber = x.RegisteredUser.PhoneNumber,
                    ImageUrl = x.RegisteredUser.ImageUrl,
                    EmailConfirmed = x.RegisteredUser.EmailConfirmed
                }
            })
            .ToListAsync(cancellationToken: cancellationToken);

        return Ok(registrations);
    }
}