using EventsManager.Server.Data;
using EventsManager.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace EventsManager.Server.Controllers;

[Route("webhook")]
[ApiController]
public class WebhookController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public WebhookController(IConfiguration configuration, ApplicationDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }
    
    // This is your Stripe CLI webhook secret for testing your endpoint locally.
    //const string endpointSecret = "whsec_d772b04c699de5068fea60dbf46f6b42729ca644643f02d3eeace68d81f7c6a7";

    [HttpPost]
    public async Task<IActionResult> Index()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        try
        {
            var endpointSecret = _configuration["StripeWebhookSecret"];
            var stripeEvent = EventUtility.ConstructEvent(json,
                Request.Headers["Stripe-Signature"], endpointSecret);

            if (stripeEvent.Type == Events.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Session;
                var registrationId = session.Metadata["RegistrationId"];
                var registration = await _context.Registrations.SingleAsync(x => x.Id == Guid.Parse(registrationId));
                registration.PaymentStatus = PaymentStatus.Paid;
                registration.State = RegistrationState.Accepted;
                await _context.SaveChangesAsync();
                var maxBibNumber = await _context.Registrations
                    .Where(x => x.Event.Id == registration.Event.Id)
                    .MaxAsync(x => x.Bib);
                registration.Bib = maxBibNumber + 1;
                await _context.SaveChangesAsync();
            }

            // Handle the event
            if (stripeEvent.Type == Events.PaymentIntentCreated)
            {
                Console.WriteLine("PAID!!!!!!!!!!" + stripeEvent);
            }
            else if (stripeEvent.Type == Events.CheckoutSessionAsyncPaymentSucceeded)
            {
            }
            // ... handle other event types
            else
            {
                Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
            }

            return Ok();
        }
        catch (StripeException e)
        {
            return BadRequest(e.Message);
        }
    }
}