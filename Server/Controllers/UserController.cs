using System.Security.Claims;
using EventsManager.Server.Data;
using EventsManager.Server.Handlers.Commands.User.DeleteUserImage;
using EventsManager.Server.Handlers.Commands.User.UpdateMyUser;
using EventsManager.Server.Handlers.Commands.User.UploadUserImage;
using EventsManager.Server.Handlers.Queries;
using EventsManager.Shared;
using EventsManager.Shared.Dtos;
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

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
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
}

public class GetAllUsersQueryRequest : IRequest<List<UserDto>>
{
    public readonly string? UserId;

    public GetAllUsersQueryRequest(string? userId)
    {
        UserId = userId;
    }
}   

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQueryRequest, List<UserDto>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetAllUsersQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<List<UserDto>> Handle(GetAllUsersQueryRequest request, CancellationToken cancellationToken)
    {
        var users = await _dbContext.Users.Select(x => new UserDto
            {
                Id = x.Id,
                UserName = x.UserName,
                Email = x.Email,
                ImageUrl = x.ImageUrl
            })
            .ToListAsync(cancellationToken: cancellationToken);

        return users;
    }
}   