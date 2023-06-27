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
            .Include(x => x.Prices)
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

        // Check if the end date is before the open registration date
        var registrationStart = sportEvent.OpenRegistrationsDate;
        if (priceRequest.EndDate.ToUniversalTime() < registrationStart)
        {
            return Conflict("You can't add a price that ends before the open registration date.");
        }

        sportEvent.Prices.Add(new EventPrice(priceRequest.Price, priceRequest.EndDate.ToUniversalTime()));
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

        // Check if the deletion of the price would leave the registration period uncovered
        var registrationEnd = price.Event.CloseRegistrationsDate;

        var otherPricesCoveringPeriod = price.Event.Prices
            .Any(p => p.Id != priceId && p.EndDate >= registrationEnd);

        if (!otherPricesCoveringPeriod)
        {
            return Conflict("You can't remove this price as it would leave the registration period uncovered");
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
            .Include(x => x.Event)
            .ThenInclude(x => x.Prices)
            .SingleAsync(x => x.Id == priceId, cancellationToken);

        if(price.Event.Owners.All(o => o.Id != userId) && price.Event.CreatorId != userId)   
        {
            return Unauthorized();
        }

        // Get the registration start and end dates
        var registrationStart = price.Event.OpenRegistrationsDate;
        var registrationEnd = price.Event.CloseRegistrationsDate;

        // Check if the updated end date is before the open registration date
        if (priceRequest.EndDate.ToUniversalTime() < registrationStart)
        {
            return Conflict("You can't update this price as the end date is before the open registration date.");
        }

        // Create the new price
        var newPrice = new EventPrice(priceRequest.Price , priceRequest.EndDate.ToUniversalTime());

        // Get the other prices
        var otherPrices = price.Event.Prices.Where(p => p.Id != priceId).ToList();

        // Add the new price to the list
        otherPrices.Add(newPrice);

        // Check if the registration period is fully covered
        var registrationCovered = otherPrices.Any(p => p.EndDate >= registrationEnd);

        if (!registrationCovered)
        {
            return Conflict("You can't update this price as it would leave the registration period uncovered");
        }

        // If all checks pass, update the price
        price.Price = priceRequest.Price;
        price.EndDate = priceRequest.EndDate.ToUniversalTime();
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }


}
