using EventsManager.Server.Handlers.Queries.Events.GetAll;
using MediatR;
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
}