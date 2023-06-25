using EventsManager.Server.Data;
using EventsManager.Server.Models;
using EventsManager.Shared.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsManager.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class SuggestionsController: ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SuggestionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [Authorize(Roles = "User")] 
    public async Task<IActionResult> PostASuggestion([FromBody] SuggestionRequest request)
    {
        var suggestion = new Suggestion(request.Content);
        
        
        return Ok(); 
    }
}