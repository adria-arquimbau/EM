using Newtonsoft.Json.Linq;

namespace EventsManager.Server.Models;

public class Payment
{
    public Guid Id { get; set; }
    public DateTime CreationDate { get; private set; }
    public DateTime StripeCreationDate { get; private set; }
    public PaymentResult Result { get; set; }
    public string Type { get; set; }
    public string Message { get; set; }
    public Registration Registration { get; set; }
    
    private Payment(){}
    
    public Payment(string type, DateTime stripeCreationDate, PaymentResult result, string message)
    {
        Type = type;
        StripeCreationDate = stripeCreationDate.ToUniversalTime();
        Result = result;
        Message = message;
        CreationDate = DateTime.UtcNow; 
    }   
}

public enum PaymentResult
{
    Failed,
    Succeeded,
    CheckoutSessionCompleted,
    PaymentIntentCreated,
    CheckoutSessionExpired
}
