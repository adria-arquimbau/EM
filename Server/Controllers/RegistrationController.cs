using System.Security.Claims;
using EventsManager.Server.Data;
using EventsManager.Server.Models;
using EventsManager.Shared.Dtos;
using EventsManager.Shared.Enums;
using EventsManager.Shared.Requests;
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
    
    [HttpPost("event/{eventId:guid}/{registrationRole}/password/{password?}")]
    [Authorize(Roles = "User")] 
    public async Task<IActionResult> Register([FromRoute] Guid eventId, [FromRoute] RegistrationRole registrationRole, [FromRoute] string? password, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _context.Users.SingleAsync(x => x.Id == userId, cancellationToken: cancellationToken);
        var eventToRegister = await _context.Events
            .Include(x => x.Registrations)
            .ThenInclude(x => x.User)
            .SingleAsync(x => x.Id == eventId, cancellationToken: cancellationToken);

        if (eventToRegister.Registrations.Any(x => x.User.Id == userId))
        {
            return BadRequest("User already registered for this event");
        }

        var registrationRolePassword = await _context.RegistrationRolePasswords
            .Where(x => x.Event.Id == eventId && x.Role == registrationRole)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (registrationRolePassword != null && registrationRolePassword.Password != password)
        {
            return BadRequest("Invalid role registration password");
        }
        
        var registration = new Registration(user, registrationRole, RegistrationState.PreRegistered, eventToRegister);
        
        _context.Registrations.Add(registration);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Ok(); 
    }
    
    [HttpPut]
    [Authorize(Roles = "Organizer")] 
    public async Task<IActionResult> Update([FromBody] RegistrationUpdateRequest registrationUpdateRequest, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var registration = await _context.Registrations
            .Include(x => x.Event)
            .ThenInclude(x => x.Owner)
            .SingleAsync(x => x.Id == registrationUpdateRequest.Id, cancellationToken: cancellationToken);

        if (registration.Event.Owner.Id != userId)
        {
            return Forbid();
        }

        registration.Update(registrationUpdateRequest.Bib, registrationUpdateRequest.CheckedIn, registrationUpdateRequest.State);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Ok(); 
    }
    
    [HttpGet("event/{eventId:guid}")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> GetMyRegistration([FromRoute] Guid eventId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var registrationDto = await _context.Registrations
            .Where(x => x.Event.Id == eventId && x.User.Id == userId)
            .Select(x => new RegistrationDto
            {
                Id = x.Id,
                CreationDate = x.CreationDate,
                Role = x.Role,
                State = x.State,
                Bib = x.Bib,    
                CheckedIn = x.CheckedIn
            })
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        return Ok(registrationDto);
    }
    
    [HttpDelete("{registrationId:guid}")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Delete([FromRoute] Guid registrationId, CancellationToken cancellationToken)
    {
        var registration = await _context.Registrations
            .Include(x => x.Event)
            .ThenInclude(x => x.Owner)
            .SingleAsync(x => x.Id == registrationId, cancellationToken: cancellationToken);

        if (registration.Event.Owner.Id != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
        {
            return Forbid();
        }
        
        _context.Registrations.Remove(registration);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }
}