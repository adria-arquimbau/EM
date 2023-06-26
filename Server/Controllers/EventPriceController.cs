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
        
        // Check if the deletion of the price would leave the registration period uncovered
        var registrationStart = price.Event.OpenRegistrationsDate;
        var registrationEnd = price.Event.CloseRegistrationsDate;
        var otherPrices = price.Event.Prices.Where(p => p.Id != priceId).ToList();

        var pricesDuringRegistration = otherPrices
            .Where(p => p.StartDate <= registrationEnd && p.EndDate >= registrationStart)
            .OrderBy(p => p.StartDate)
            .ToList();

        if (pricesDuringRegistration.Count == 0 ||
            pricesDuringRegistration.First().StartDate > registrationStart ||
            pricesDuringRegistration.Last().EndDate < registrationEnd)
        {
            return Conflict("You can't remove this price as it would leave the registration period uncovered");
        }

        for (var i = 0; i < pricesDuringRegistration.Count - 1; i++)
        {
            if (pricesDuringRegistration[i].EndDate < pricesDuringRegistration[i + 1].StartDate)
            {
                return Conflict("You can't remove this price as it would leave the registration period uncovered");
            }
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

        // Prepare the new price
        var newPrice = new EventPrice(priceRequest.Price ,priceRequest.StartDate.ToUniversalTime(), priceRequest.EndDate.ToUniversalTime());

        // Get the registration period dates
        var registrationStart = price.Event.OpenRegistrationsDate;
        var registrationEnd = price.Event.CloseRegistrationsDate;

        // Get the other prices
        var otherPrices = price.Event.Prices.Where(p => p.Id != priceId).ToList();

        // Add the new price to the list
        otherPrices.Add(newPrice);

        // Filter prices that are active during the registration period and order them
        var pricesDuringRegistration = otherPrices
            .Where(p => p.StartDate <= registrationEnd && p.EndDate >= registrationStart)
            .OrderBy(p => p.StartDate)
            .ToList();

        // Check if the registration period is fully covered
        if (pricesDuringRegistration.Count == 0 ||
            pricesDuringRegistration.First().StartDate > registrationStart ||
            pricesDuringRegistration.Last().EndDate < registrationEnd)
        {
            return Conflict("You can't update this price as it would leave the registration period uncovered");
        }

        // Check for gaps between consecutive prices
        for (var i = 0; i < pricesDuringRegistration.Count - 1; i++)
        {
            if (pricesDuringRegistration[i].EndDate < pricesDuringRegistration[i + 1].StartDate)
            {
                return Conflict("You can't update this price as it would leave the registration period uncovered");
            }
        }

        // If all checks pass, update the price
        price.Price = priceRequest.Price;
        price.StartDate = priceRequest.StartDate.ToUniversalTime();
        price.EndDate = priceRequest.EndDate.ToUniversalTime();
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }

}
