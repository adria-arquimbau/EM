using Newtonsoft.Json.Linq;

namespace EventsManager.Server.Models;

public class Payment
{
    public Guid Id { get; set; }
    public DateTime CreationDate { get; private set; }
    public DateTime StripeCreationDate { get; private set; }
    public string EventRawJObject { get; private set; }
    public string Type { get; set; }
    public Registration Registration { get; set; }
    
    private Payment(){}
    
    public Payment(string type, DateTime stripeCreationDate, string eventRawJObject)
    {
        Type = type;
        StripeCreationDate = stripeCreationDate.ToUniversalTime();
        EventRawJObject = eventRawJObject;
        CreationDate = DateTime.UtcNow;
    }   
}
