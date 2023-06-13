using Microsoft.AspNetCore.Authorization;
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
    public ActionResult Create()
    {
        var domain = _configuration["Domain"];
        
        string customerEmail = "customer@gmail.com";
    
        var customerService = new CustomerService();
        var customerOptions = new CustomerCreateOptions
        {
            Email = customerEmail,
        };

        // Try to find the customer
        var customers = customerService.List(new CustomerListOptions { Email = customerEmail });
        Customer customer = customers.FirstOrDefault();
    
        // If the customer does not exist, create a new one
        if (customer == null)
        {
            customer = customerService.Create(customerOptions);
        }
        
        var options = new SessionCreateOptions
        {
            Customer = customer.Id,
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