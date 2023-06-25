using System.Security.Claims;
using EventsManager.Server.Data;
using EventsManager.Server.Models;
using EventsManager.Shared.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class SuggestionController: ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SuggestionController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [Authorize(Roles = "User")] 
    public async Task<IActionResult> PostASuggestion([FromBody] SuggestionRequest request, CancellationToken cancellationToken)
    {
        var requesterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _context.Users
            .Where(x => x.Id == requesterId)
            .SingleAsync(cancellationToken: cancellationToken);
        
        var suggestion = new Suggestion(request.Content, user);
        
        _context.Suggestions.Add(suggestion);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Ok(); 
    }
}