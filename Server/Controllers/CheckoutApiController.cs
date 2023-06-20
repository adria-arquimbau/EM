using EventsManager.Server.Data;
using EventsManager.Shared.Enums;
using EventsManager.Shared.Requests;
using EventsManager.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace EventsManager.Server.Controllers;

[Route("create-checkout-session")]
[ApiController]
public class CheckoutApiController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public CheckoutApiController(IConfiguration configuration, ApplicationDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }
    
    [HttpPost]
    public async Task<ActionResult> Create([FromBody]CheckoutRequest request, CancellationToken cancellationToken)
    {
        var eventMaxRegistrations = await _context.Events
            .Where(x => x.Id == Guid.Parse(request.EventId))
            .Select(x => x.MaxRegistrations)
            .SingleAsync(cancellationToken);
        
        var ridersAcceptedCount = await _context.Registrations
            .Where(x => x.State == RegistrationState.Accepted && x.Role == RegistrationRole.Rider)
            .CountAsync(cancellationToken);

        if (eventMaxRegistrations <= ridersAcceptedCount)
        {
            return BadRequest("No more registrations allowed for this event.");
        }
        
        var domain = _configuration["Domain"];

        long currentPrice;  
        try
        {
            currentPrice = await CalculatePriceForTheWeek(request.EventId);
        }
        catch (Exception e)
        {
            return NotFound(e.Message);
        }
       
        var priceOptions = new PriceCreateOptions
        {
            UnitAmount = currentPrice, // This method will return the price for the current week
            Currency = "eur",
            Product = "prod_O58nBZHZmazZ7w", // Replace with your Product ID
        };
        var priceService = new PriceService();
        var stripePrice = await priceService.CreateAsync(priceOptions);

        var sessionOptions = new SessionCreateOptions
        {
            LineItems = new List<SessionLineItemOptions>    
            {
                new()
                {
                    Price = stripePrice.Id,
                    Quantity = 1,
                }
            },
            Mode = "payment",
            SuccessUrl = domain + $"/event-detail/{request.EventId}",
            CancelUrl = domain + $"/event-detail/{request.EventId}",
            Metadata = new Dictionary<string, string>
            {
                { "RegistrationId", request.RegistrationId }
            }
        };
    
        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(sessionOptions);

        return Ok(new CheckoutResponse
        {
            Url = session.Url
        });
    }

    private async Task<long> CalculatePriceForTheWeek(string eventId)
    {
        var now = DateTime.UtcNow;
    
        var price = await _context.EventPrices
            .Where(x => x.Event.Id == Guid.Parse(eventId))
            .SingleOrDefaultAsync(x => x.StartDate <= now && x.EndDate >= now);

        if (price == null)
        {
            throw new NullReferenceException("Price not found");
        }
        
        return (long)(price.Price * 100);
    }
}
