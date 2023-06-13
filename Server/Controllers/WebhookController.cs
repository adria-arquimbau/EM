using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace EventsManager.Server.Controllers;

[Route("webhook")]
[ApiController]
public class WebhookController : Controller
{
        
    // This is your Stripe CLI webhook secret for testing your endpoint locally.
    const string endpointSecret = "whsec_d772b04c699de5068fea60dbf46f6b42729ca644643f02d3eeace68d81f7c6a7";

    [HttpPost]
    public async Task<IActionResult> Index()
    {
        Console.WriteLine("TRIGGERED");
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json,
                Request.Headers["Stripe-Signature"], endpointSecret);

            if (stripeEvent.Type == Events.PaymentIntentSucceeded)
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                Console.WriteLine("PaymentIntent was successful!");
                Console.WriteLine("PaymentIntent ID: {0}", paymentIntent.Id);
                // log any other details you're interested in
            }
            // Handle the event
            if (stripeEvent.Type == Events.PaymentIntentCreated)
            {
                Console.WriteLine("PAID!!!!!!!!!!" + stripeEvent);
            }
            else if (stripeEvent.Type == Events.CheckoutSessionAsyncPaymentSucceeded)
            {
            }
            else if (stripeEvent.Type == Events.CheckoutSessionCompleted)
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
            return BadRequest();
        }
    }
}