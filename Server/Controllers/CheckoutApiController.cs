using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
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
    public ActionResult Create([FromForm]string userId)
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
            SuccessUrl = domain + "/success.html",
            CancelUrl = domain + "/cancel.html",
            Metadata = new Dictionary<string, string>
            {
                { "UserId", userId }
            }
        };
        var service = new SessionService();
        var session = service.Create(options);

        Response.Headers.Add("Location", session.Url);
        return new StatusCodeResult(303);
    }
}