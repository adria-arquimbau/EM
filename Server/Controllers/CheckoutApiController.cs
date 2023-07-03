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
        var registration = await _context.Registrations
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.Id == Guid.Parse(request.RegistrationId), cancellationToken);

        if (registration == null)
        {
            return NotFound("Registration not found");
        }
        
        var isRegistrationAllowed = await _context.Events
            .Where(x => x.Id == Guid.Parse(request.EventId))
            .Select(x => x.MaxRegistrations > _context.Registrations.Count(r => r.Event.Id == x.Id && r.State == RegistrationState.Accepted && r.Role == RegistrationRole.Rider))
            .SingleAsync(cancellationToken);

        if (!isRegistrationAllowed)
        {
            return BadRequest("No more registrations allowed for this event.");
        }
        
        var productId = await _context.Events
            .Where(x => x.Id == Guid.Parse(request.EventId))
            .Select(x => x.ProductId)
            .SingleAsync(cancellationToken);

        var domain = _configuration["Domain"];

        long currentPrice;  
        try
        {
            currentPrice = await CalculatePriceForTheWeek(request.EventId, cancellationToken);
        }
        catch (Exception e)
        {
            return NotFound(e.Message);
        }
       
        var priceOptions = new PriceCreateOptions
        {
            UnitAmount = currentPrice,
            Currency = "eur",
            Product = productId
        };
        var priceService = new PriceService();
        var stripePrice = await priceService.CreateAsync(priceOptions, cancellationToken: cancellationToken);

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
            CustomerEmail = registration.User.Email,
            SuccessUrl = domain + $"/event-detail/{request.EventId}",
            CancelUrl = domain + $"/event-detail/{request.EventId}",
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    { "RegistrationId", request.RegistrationId },
                }
            },
            Metadata = new Dictionary<string, string>
            {
                { "RegistrationId", request.RegistrationId }
            },
            ExpiresAt = DateTime.UtcNow + new TimeSpan(0, 30, 0)
        };
    
        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(sessionOptions, cancellationToken: cancellationToken);

        return Ok(new CheckoutResponse
        {
            Url = session.Url
        });
    }

    private async Task<long> CalculatePriceForTheWeek(string eventId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var currentEvent = await _context.Events
            .Where(x => x.Id == Guid.Parse(eventId))
            .Include(x => x.Prices)
            .SingleAsync(cancellationToken);
    
        var prices = currentEvent.Prices.OrderBy(p => p.EndDate).ToList();
        var currentPriceIndex = prices.FindIndex(p => now >= (p == prices.First() ? currentEvent.OpenRegistrationsDate : prices[prices.IndexOf(p) - 1].EndDate) && now <= p.EndDate);

        if (currentPriceIndex == -1 || currentEvent.OpenRegistrationsDate > now)
        {
            throw new NullReferenceException("Price not found");
        }

        var price = prices[currentPriceIndex];
    
        return (long)(price.Price * 100);
    }
}
