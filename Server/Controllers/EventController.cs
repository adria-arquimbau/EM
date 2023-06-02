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
}