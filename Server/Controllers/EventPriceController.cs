using System.Security.Claims;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EventsManager.Server.Data;
using EventsManager.Server.Models;
using EventsManager.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class EventPriceController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EventPriceController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    [HttpPost]
    [Authorize(Roles = "Organizer")]
    public async Task<ActionResult> Add([FromBody] EventPriceDto priceRequest, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
     
        var sportEvent = await _context.Events
            .Include(x => x.Owner)
            .SingleAsync(x => x.Id == priceRequest.EventId, cancellationToken);

        if(sportEvent.Owner.Id != userId)   
        {
            return Unauthorized();
        }
        
        sportEvent.Prices.Add(new EventPrice(priceRequest.Price, priceRequest.StartDate, priceRequest.EndDate));
        await _context.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }       
}
