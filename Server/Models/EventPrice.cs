namespace EventsManager.Server.Models;

public class EventPrice
{
    public Guid Id { get; set; }
    public decimal Price { get; set; }
    public DateTime EndDate { get; set; }
    public Event Event { get; set; }

    private EventPrice() { }

    public EventPrice(decimal price, DateTime endDate)
    {
        Price = price;
        EndDate = endDate;  
    }   
}