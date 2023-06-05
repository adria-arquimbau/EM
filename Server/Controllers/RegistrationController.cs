﻿using System.Security.Claims;
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
    public async Task<IActionResult> Register([FromRoute] Guid eventId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _context.Users.SingleAsync(x => x.Id == userId);
        var eventToRegister = await _context.Events.SingleAsync(x => x.Id == eventId);

        var registration = new Registration(user.Id, RegistrationRole.Rider, RegistrationState.PreRegistered, eventToRegister.Id);
        
        _context.Registrations.Add(registration);
        await _context.SaveChangesAsync();
        
        return Ok(); 
    }
}