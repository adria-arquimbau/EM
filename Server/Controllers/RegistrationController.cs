using System.Security.Claims;
using EventsManager.Server.Data;
using EventsManager.Server.Models;
using EventsManager.Shared.Dtos;
using EventsManager.Shared.Enums;
using EventsManager.Shared.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class RegistrationController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public RegistrationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
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
        
        var currentDate = DateTime.UtcNow;
        if (currentDate < eventToRegister.OpenRegistrationsDate || 
            currentDate > eventToRegister.CloseRegistrationsDate)
        {
            return BadRequest("Registrations are currently closed for this event");
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
            .Include(x => x.User)
            .Include(x => x.Event)
            .ThenInclude(x => x.Owners)
            .SingleAsync(x => x.Id == registrationUpdateRequest.Id, cancellationToken: cancellationToken);

        if (registration.Event.Owners.All(o => o.Id != userId))
        {
            return Forbid();
        }
        
        if (registration.Event.CreatorId == registration.User.Id)
        {
            return BadRequest("Cannot update creator registration");
        }
        
        var oldRegistrationRole = registration.Role.ToString();
        var oldRegistrationState = registration.State.ToString();
        
        registration.Update(registrationUpdateRequest.Bib, registrationUpdateRequest.CheckedIn, registrationUpdateRequest.State);

        if (registration.Role == RegistrationRole.Staff && registrationUpdateRequest.State == RegistrationState.Accepted)
        {
            var user = await _userManager.FindByIdAsync(registration.User.Id);
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Staff"))
            {
                await _userManager.AddToRoleAsync(user, "Staff");
            }
        }   
        else
        {
            if (oldRegistrationRole == RegistrationRole.Staff.ToString() && oldRegistrationState == RegistrationState.Accepted.ToString())
            {
                var anotherStaffAcceptedRegistrationsCount = await _context.Registrations
                    .Where(x => (x.State == RegistrationState.Accepted && x.Role == RegistrationRole.Staff && x.User.Id == registration.User.Id) &&
                                x.Id != registrationUpdateRequest.Id)
                    .CountAsync(cancellationToken: cancellationToken);

                if (anotherStaffAcceptedRegistrationsCount == 0)
                {
                    var user = await _userManager.FindByIdAsync(registration.User.Id);
                    await _userManager.RemoveFromRoleAsync(user, "Staff");
                }   
            }   
        }
        
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
            .Include(x => x.User)
            .Select(x => new RegistrationDto
            {
                Id = x.Id,
                CreationDate = x.CreationDate,
                Role = x.Role,
                State = x.State,
                Bib = x.Bib,    
                CheckedIn = x.CheckedIn,
                PaymentStatus = x.PaymentStatus,
            })
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        return Ok(registrationDto);
    }
    
    [HttpDelete("{registrationId:guid}")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Delete([FromRoute] Guid registrationId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var registration = await _context.Registrations
            .Include(x => x.User)
            .Include(x => x.Event)
            .ThenInclude(x => x.Owners)
            .SingleAsync(x => x.Id == registrationId, cancellationToken: cancellationToken);

        if (registration.Event.Owners.All(o => o.Id != userId))
        {
            return Forbid();
        }
        
        if (registration is { State: RegistrationState.Accepted, Role: RegistrationRole.Staff })
        {
            var anotherStaffAcceptedRegistrationsCount = await _context.Registrations
                .Where(x => (x.State == RegistrationState.Accepted && x.Role == RegistrationRole.Staff && x.User.Id == registration.User.Id) &&
                            x.Id != registrationId)
                .CountAsync(cancellationToken: cancellationToken);

            if (anotherStaffAcceptedRegistrationsCount == 0)
            {
                var user = await _userManager.FindByIdAsync(registration.User.Id);
                await _userManager.RemoveFromRoleAsync(user, "Staff");
            }   
        }

        if (registration.Event.CreatorId == registration.User.Id)
        {
            return BadRequest("Cannot delete creator registration");
        }
        
        _context.Registrations.Remove(registration);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }
}