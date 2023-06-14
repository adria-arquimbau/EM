namespace EventsManager.Server.Models;

public class EventPrice
{
    public Guid Id { get; set; }
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Event Event { get; set; }

    private EventPrice() { }

    public EventPrice(decimal price, DateTime startDate, DateTime endDate)
    {
        Price = price;
        StartDate = startDate;
        EndDate = endDate;
    }   
}