using EventsManager.Server.Data;
using EventsManager.Server.Models;
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
            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], endpointSecret);
            
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            var registrationId = paymentIntent?.Metadata["RegistrationId"];
            var registration = await _context.Registrations
                .Include(x => x.Event)
                .SingleAsync(x => x.Id == Guid.Parse(registrationId));

            if (stripeEvent.Type == Events.PaymentIntentSucceeded)
            {   
                registration.PaymentStatus = PaymentStatus.Paid;
                registration.State = RegistrationState.Accepted;
                registration.Price = paymentIntent.Amount / 100.0m;
    
                var maxBibNumber = await _context.Registrations
                    .Where(x => x.Event.Id == registration.Event.Id && x.Bib != null)
                    .MaxAsync(x => x.Bib);

                if (maxBibNumber == null)
                {
                    registration.Bib = 1;
                }
                if (maxBibNumber != null)
                {
                    registration.Bib = maxBibNumber + 1;
                }
                
                var message = $"Payment successful. PaymentIntent ID: {paymentIntent.Id}.";
                registration.Payments.Add(new Payment(stripeEvent.Type, stripeEvent.Created, PaymentResult.Succeeded, message));
                await _context.SaveChangesAsync();
            }
            else if (stripeEvent.Type == Events.PaymentIntentPaymentFailed)
            {
                var message = $"Payment failed. Error: {paymentIntent?.LastPaymentError?.Message}.";
                registration.Payments.Add(new Payment(stripeEvent.Type, stripeEvent.Created, PaymentResult.Failed, message));
                await _context.SaveChangesAsync();
            }
            else if (stripeEvent.Type == Events.PaymentIntentCreated)
            {
                var message = $"Payment intent created: {paymentIntent?.Id}.";
                registration.Payments.Add(new Payment(stripeEvent.Type, stripeEvent.Created, PaymentResult.PaymentIntentCreated, message));
                await _context.SaveChangesAsync();
            }
            else if (stripeEvent.Type == Events.CheckoutSessionExpired)
            {
                var session = stripeEvent.Data.Object as Session;
                var message = $"Checkout session expired. Session id: {session?.Id}.";
                registration.Payments.Add(new Payment(stripeEvent.Type, stripeEvent.Created, PaymentResult.CheckoutSessionExpired, message));
                await _context.SaveChangesAsync();
            }
            else
            {
                var message = $"Event {stripeEvent.Type} was not explicitly handled.";
                registration.Payments.Add(new Payment(stripeEvent.Type, stripeEvent.Created, PaymentResult.Failed, message));
                await _context.SaveChangesAsync();
            }
            
            return Ok();
        }
        catch (StripeException e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost("checkout-session-completed")]
    public async Task<IActionResult> CheckoutSessionCompleted()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        try
        {
            var endpointSecret = _configuration["CheckoutSessionCompleted"];
            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], endpointSecret);
            var session = stripeEvent.Data.Object as Session;
            
            if (stripeEvent.Type == Events.CheckoutSessionCompleted)
            {
                var registrationId = session?.Metadata["RegistrationId"];
                var registration = await _context.Registrations
                    .Include(x => x.Event)
                    .SingleAsync(x => x.Id == Guid.Parse(registrationId));
                var message = $"Checkout session completed. Session id: {session?.Id}.";
                registration.Payments.Add(new Payment(stripeEvent.Type, stripeEvent.Created, PaymentResult.CheckoutSessionCompleted, message));
                await _context.SaveChangesAsync();
            }

            return Ok();
        }
        catch (StripeException e)
        {
            return BadRequest(e.Message);
        }
    }
}
