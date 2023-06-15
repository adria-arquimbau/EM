using System.Security.Claims;
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
            .Include(x => x.Owners)
            .SingleAsync(x => x.Id == priceRequest.EventId, cancellationToken);

        if(sportEvent.Owners.All(o => o.Id != userId) && sportEvent.CreatorId != userId)   
        {
            return Unauthorized();
        }

        if (sportEvent.IsFree)
        {
            return Conflict("Event is free. You can't add a price");
        }
        
        if (priceRequest.Price < 0.5m)
        {
            return Conflict("Price must be greater than 0.5");
        }

        sportEvent.Prices.Add(new EventPrice(priceRequest.Price, priceRequest.StartDate.ToUniversalTime(), priceRequest.EndDate.ToUniversalTime()));
        await _context.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }    
    
    [HttpDelete("{priceId:guid}")]
    [Authorize(Roles = "Organizer")]
    public async Task<ActionResult> Remove([FromRoute] Guid priceId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
     
        var price = await _context.EventPrices
            .Include(x => x.Event)
            .ThenInclude(x => x.Owners)
            .Include(x => x.Event)
            .ThenInclude(x => x.Creator)
            .Include(x => x.Event)
            .ThenInclude(x => x.Prices)
            .SingleAsync(x => x.Id == priceId, cancellationToken);

        if(price.Event.Owners.All(o => o.Id != userId) && price.Event.CreatorId != userId)   
        {
            return Unauthorized();
        }

        if (price.Event.Prices.Count == 1)
        {
            return Conflict("You can't remove the last price");
        }
        
        _context.EventPrices.Remove(price);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }   
    
    [HttpPut("{priceId:guid}")]
    [Authorize(Roles = "Organizer")]
    public async Task<ActionResult> Update([FromRoute] Guid priceId,[FromBody] EventPriceDto priceRequest, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
     
        var price = await _context.EventPrices
            .Include(x => x.Event)
            .ThenInclude(x => x.Owners)
            .Include(x => x.Event)
            .ThenInclude(x => x.Creator)
            .SingleAsync(x => x.Id == priceId, cancellationToken);

        if(price.Event.Owners.All(o => o.Id != userId) && price.Event.CreatorId != userId)   
        {
            return Unauthorized();
        }

        if (priceRequest.Price < 0.5m)
        {
            return Conflict("Price must be greater than 0.5");
        }
        
        price.Price = priceRequest.Price;
        price.StartDate = priceRequest.StartDate.ToUniversalTime();
        price.EndDate = priceRequest.EndDate.ToUniversalTime();
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }   
}
