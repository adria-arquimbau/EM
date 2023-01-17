using System.Security.Claims;
using EventsManager.Server.Data;
using EventsManager.Server.Handlers.Queries;
using EventsManager.Shared.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Controllers;

[Authorize(Roles = "User")] 
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMediator _mediator;

    public UserController(ApplicationDbContext dbContext, IMediator mediator)
    {
        _dbContext = dbContext;
        _mediator = mediator;
    }   
    
    [HttpGet]
    public async Task<IActionResult> GetMyUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var response = await _mediator.Send(new GetMyUserCommandRequest(userId));
        return Ok(response); 
    }
    
    [HttpPut]   
    public async Task<IActionResult> UpdateUser([FromBody] UserDto user)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != user.Id)
        {
            return BadRequest();
        }

        var userEntity = await _dbContext.Users.SingleAsync(x => x.Id == user.Id);
        userEntity.Name = user.Name;
        userEntity.FamilyName = user.FamilyName;
        userEntity.Address = user.Address;
        userEntity.City = user.City;
        userEntity.Country = user.Country;
        userEntity.PostalCode = user.PostalCode;
        userEntity.PhoneNumber = user.PhoneNumber;
        await _dbContext.SaveChangesAsync();
        
        return Ok();
    }
}
