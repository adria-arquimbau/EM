using EventsManager.Shared.Requests;
using EventsManager.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace EventsManager.Server.Controllers;

[Route("create-checkout-session")]
[ApiController]
public class CheckoutApiController : Controller
{
    private readonly IConfiguration _configuration;

    public CheckoutApiController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    [HttpPost]
    public ActionResult Create([FromBody]CheckoutRequest request)
    {
        var domain = _configuration["Domain"];

        var priceOptions = new PriceCreateOptions
        {
            UnitAmount = CalculatePriceForTheWeek(), // This method will return the price for the current week
            Currency = "eur",
            Product = "prod_O58nBZHZmazZ7w", // Replace with your Product ID
        };
        var priceService = new PriceService();
        var stripePrice = priceService.Create(priceOptions);

        var sessionOptions = new SessionCreateOptions
        {
            LineItems = new List<SessionLineItemOptions>    
            {
                new SessionLineItemOptions
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
        var session = sessionService.Create(sessionOptions);

        return Ok(new CheckoutResponse
        {
            Url = session.Url
        });
    }

    private long CalculatePriceForTheWeek()
    {
        // Here goes your logic to calculate the price based on the current week.
        // The returned amount should be in cents.
        // For example, to charge $10.99 you would return 1099.
        return 10000;
    }
}
