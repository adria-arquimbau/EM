using System.Security.Claims;
using EventsManager.Server.Data;
using EventsManager.Server.Handlers.Commands.User.ConfirmEmail;
using EventsManager.Server.Handlers.Commands.User.DeleteUser;
using EventsManager.Server.Handlers.Commands.User.DeleteUserImage;
using EventsManager.Server.Handlers.Commands.User.RemoveOrganizer;
using EventsManager.Server.Handlers.Commands.User.SetAsOrganizer;
using EventsManager.Server.Handlers.Commands.User.UpdateMyUser;
using EventsManager.Server.Handlers.Commands.User.UploadUserImage;
using EventsManager.Server.Handlers.Queries.Users.GetAll;
using EventsManager.Server.Handlers.Queries.Users.GetMyUser;
using EventsManager.Shared;
using EventsManager.Shared.Dtos;
using EventsManager.Shared.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _dbContext;

    public UserController(IMediator mediator, ApplicationDbContext dbContext)
    {
        _mediator = mediator;
        _dbContext = dbContext;
    }   
    
    [HttpGet]
    [Authorize(Roles = "User")] 
    public async Task<IActionResult> GetMyUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var response = await _mediator.Send(new GetMyUserQueryRequest(userId));
        return Ok(response); 
    }
    
    [HttpPut]   
    [Authorize(Roles = "User")] 
    public async Task<IActionResult> UpdateUser([FromBody] UserDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await _mediator.Send(new UpdateMyUserCommandRequest(userId, request));
        return Ok();
    }
    
    [HttpPost("image")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<UploadResult>> UploadUserImage(IFormFile file) 
    {   
        var response = await _mediator.Send(new UploadUserImageCommandRequest(file, User.FindFirst(ClaimTypes.NameIdentifier)?.Value));
        return Ok(response);    
    }
    
    [HttpDelete("image")]   
    [Authorize(Roles = "User")] 
    public async Task<IActionResult> DeleteUserImage()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await _mediator.Send(new DeleteUserImageCommandRequest(userId));
        return Ok();
    }
    
    [HttpGet("all-users")]   
    [Authorize(Roles = "Administrator")] 
    public async Task<IActionResult> GetAllUsers()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var response = await _mediator.Send(new GetAllUsersQueryRequest(userId));
        return Ok(response);
    }
    
    [HttpGet("all-users-to-set-owner/event/{eventId:guid}")]   
    [Authorize(Roles = "Organizer")] 
    public async Task<IActionResult> GetAllUsersToSetOwner([FromRoute] Guid eventId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var eventEntity = await _dbContext.Events
            .Where(x => x.Id == eventId)
            .Include(x => x.Creator)
            .Include(x => x.Owners)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (eventEntity == null || eventEntity.Creator.Id != userId)
        {
            return Forbid();
        }

        var users = await _dbContext.Users.ToListAsync(cancellationToken);

        var finalList = users.Select(user => new UserDtoToSetASOwner
        {
            Id = user.Id,
            UserName = user.UserName,
            IsOwner = eventEntity.Owners.Any(x => x.Id == user.Id)
        }).ToList();

        return Ok(finalList);
    }
    
    [HttpPut("confirm-email")]   
    [Authorize(Roles = "Administrator")] 
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        await _mediator.Send(new ConfirmEmailCommandRequest(request.IdToConfirm));
        return Ok();
    }
    
    [HttpPut("{userId:guid}/set-organizer")]   
    [Authorize(Roles = "Administrator")] 
    public async Task<IActionResult> SetUserAsOrganizer([FromRoute]Guid userId)
    {   
        await _mediator.Send(new SetUserAsOrganizerCommandRequest(userId));
        return Ok();    
    }   
    
    [HttpPut("{userId:guid}/remove-organizer")]   
    [Authorize(Roles = "Administrator")] 
    public async Task<IActionResult> RemoveOrganizer([FromRoute]Guid userId)
    {   
        await _mediator.Send(new RemoveOrganizerCommandRequest(userId));
        return Ok();    
    }  
        
    [HttpDelete("{userId}")]   
    [Authorize(Roles = "Administrator")] 
    public async Task<IActionResult> DeleteUser([FromRoute]string userId)
    {
        await _mediator.Send(new DeleteUserCommandRequest(userId));
        return Ok();
    }
}