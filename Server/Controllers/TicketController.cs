using System.Security.Claims;
using EventsManager.Server.Data;
using EventsManager.Server.Models;
using EventsManager.Shared.Dtos;
using EventsManager.Shared.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class TicketController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TicketController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    
    [HttpPost("{TicketId:guid}/admin-response")]
    [Authorize(Roles = "Organizer")] 
    public async Task<IActionResult> PostAdminResponse([FromRoute] Guid ticketId, [FromBody] TicketResponseRequest request, CancellationToken cancellationToken)
    {
        var requesterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _userManager.FindByIdAsync(requesterId);
        
        var ticket = await _context.Tickets
            .Include(t => t.Registration)
            .ThenInclude(r => r.Event)
            .ThenInclude(e => e.Owners)
            .SingleAsync(t => t.Id == ticketId, cancellationToken);

        if (ticket.Registration.Event.Owners.All(o => o.Id != requesterId))
        {
            return Unauthorized();
        }

        ticket.Responses.Add(new TicketResponse(request.Text, user, true));
        await _context.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }   
    
    [HttpGet("{TicketId:guid}")]
    [Authorize(Roles = "User")] 
    public async Task<IActionResult> GetATicket([FromRoute] Guid ticketId, CancellationToken cancellationToken)
    {
        var requesterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var ticket = await _context.Tickets
            .Include(t => t.Registration)
            .ThenInclude(r => r.Event)
            .ThenInclude(e => e.Owners)
            .Include(t => t.Responses)
            .ThenInclude(r => r.RespondedBy)
            .Include(t => t.Registration)
            .ThenInclude(r => r.User)
            .SingleAsync(t => t.Id == ticketId, cancellationToken);

        if (ticket.Registration.Event.Owners.All(o => o.Id != requesterId) && ticket.Registration.User.Id != requesterId)
        {
            return Unauthorized();
        }
        
        return Ok(new TicketDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Text = ticket.Text,
            Solved = ticket.Solved,
            SolvedBy = ticket.SolvedBy?.UserName,
            CreatedBy = ticket.Registration.User.UserName,
            CreationDate = ticket.CreationDate,
            SolvedDate = ticket.SolvedDate,
            TicketResponses = ticket.Responses.Select(r => new TicketResponseDto
                {
                    Text = r.Text,
                    ResponseDate = r.ResponseDate,
                    IsAdminResponse = r.IsAdminResponse,
                    RespondedBy = r.RespondedBy.UserName
                })
                .ToList()
        });
    }   
    
    [HttpGet("{TicketId:guid}/solve/{solved:bool}")]
    [Authorize(Roles = "Organizer")] 
    public async Task<IActionResult> SolveTicket([FromRoute] Guid ticketId, [FromRoute] bool solved, CancellationToken cancellationToken)
    {
        var requesterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _userManager.FindByIdAsync(requesterId);
        
        var ticket = await _context.Tickets
            .Include(t => t.Registration)
            .ThenInclude(r => r.Event)
            .ThenInclude(e => e.Owners)
            .SingleAsync(t => t.Id == ticketId, cancellationToken);

        if (ticket.Registration.Event.Owners.All(o => o.Id != requesterId))
        {
            return Unauthorized();
        }

        ticket.Solved = solved;
        ticket.SolvedBy = solved ? user : null;
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }   
}
