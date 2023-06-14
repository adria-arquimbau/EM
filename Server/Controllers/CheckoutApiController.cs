using EventsManager.Shared.Requests;
using EventsManager.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
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
        var options = new SessionCreateOptions
        {
            LineItems = new List<SessionLineItemOptions>    
            {
                new SessionLineItemOptions
                {
                    // Provide the exact Price ID (for example, pr_1234) of the product you want to sell
                    Price = "price_1NIIMUKiJO2GrIfAT09sZEDL",
                    Quantity = 1
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
        var service = new SessionService();
        var session = service.Create(options);

        return Ok(new CheckoutResponse
        {
            Url = session.Url
        });
    }
}
