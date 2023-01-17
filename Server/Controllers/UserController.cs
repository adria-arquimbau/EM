using System.Security.Claims;
using EventsManager.Server.Handlers.Commands.User.DeleteUserImage;
using EventsManager.Server.Handlers.Commands.User.UpdateMyUser;
using EventsManager.Server.Handlers.Commands.User.UploadUserImage;
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
    
    [HttpPost("image")]
    public async Task<IActionResult> UploadUserImage(IFormFile files) 
    {   
        await _mediator.Send(new UploadUserImageCommandRequest(files, User.FindFirst(ClaimTypes.NameIdentifier)?.Value));
        return Ok();  
    }
    
    [HttpDelete("image")]   
    public async Task<IActionResult> DeleteUserImage()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await _mediator.Send(new DeleteUserImageCommandRequest(userId));
        return Ok();
    }
}