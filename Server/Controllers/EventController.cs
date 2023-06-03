using System.Security.Claims;
using EventsManager.Server.Handlers.Commands.Events.Create;
using EventsManager.Server.Handlers.Commands.Events.Delete;
using EventsManager.Server.Handlers.Queries.Events.GetAll;
using EventsManager.Server.Handlers.Queries.Events.GetMyEvents;
using EventsManager.Shared.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsManager.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class EventController : ControllerBase
{
    private readonly IMediator _mediator;

    public EventController(IMediator mediator)
    {
        _mediator = mediator;
    }   
    
    [HttpGet]
    public async Task<IActionResult> GetAllEvents()
    {
        var response = await _mediator.Send(new GetAllEventsQueryRequest());
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
}