using System.Security.Claims;
using EventsManager.Server.Handlers.Commands.UpdateMyUser;
using EventsManager.Server.Handlers.Queries;
using EventsManager.Shared.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsManager.Server.Controllers;

[Authorize(Roles = "User")] 
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }   
    
    [HttpGet]
    public async Task<IActionResult> GetMyUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var response = await _mediator.Send(new GetMyUserQueryRequest(userId));
        return Ok(response); 
    }
    
    [HttpPut]   
    public async Task<IActionResult> UpdateUser([FromBody] UserDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await _mediator.Send(new UpdateMyUserCommandRequest(userId, request));
        return Ok();
    }
}
