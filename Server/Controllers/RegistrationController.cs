using System.Security.Claims;
using Duende.IdentityServer;
using EventsManager.Server.Data;
using EventsManager.Server.Handlers.Commands.Events.Create;
using EventsManager.Server.Models;
using EventsManager.Shared.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Controllers;


[ApiController]
[Route("[controller]")]
public class RegistrationController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RegistrationController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    [HttpPost("event/{eventId:guid}")]
    [Authorize(Roles = "User")] 
    public async Task<IActionResult> Register([FromRoute] Guid eventId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _context.Users.SingleAsync(x => x.Id == userId, cancellationToken: cancellationToken);
        var eventToRegister = await _context.Events
            .Include(x => x.Registrations)
            .SingleAsync(x => x.Id == eventId, cancellationToken: cancellationToken);

        if (eventToRegister.Registrations.Any(x => x.RegisteredUser.Id == userId))
            return BadRequest("User already registered for this event");
        
        var registration = new Registration(user.Id, RegistrationRole.Rider, RegistrationState.PreRegistered, eventToRegister.Id);
        
        _context.Registrations.Add(registration);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Ok(); 
    }
    
    [HttpGet("event/{eventId:guid}/iam-registered")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> IAmRegistered([FromRoute] Guid eventId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var registration = await _context.Registrations
            .Where(x => x.EventId == eventId && x.RegisteredUserId == userId)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        return registration != null ? Ok() : NotFound();
    }
}
