using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace EventsManager.Server.Controllers;

[Route("create-checkout-session")]
[ApiController]
public class CheckoutApiController : Controller
{
    [HttpPost]
    public ActionResult Create()
    {
        const string domain = "https://localhost:7008";
        var options = new SessionCreateOptions
        {
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    // Provide the exact Price ID (for example, pr_1234) of the product you want to sell
                    Price = "price_1NIIMUKiJO2GrIfAT09sZEDL",
                    Quantity = 1
                },
            },
            Mode = "payment",
            SuccessUrl = domain + "/success.html",
            CancelUrl = domain + "/cancel.html",
        };
        var service = new SessionService();
        var session = service.Create(options);

        Response.Headers.Add("Location", session.Url);
        return new StatusCodeResult(303);
    }
}