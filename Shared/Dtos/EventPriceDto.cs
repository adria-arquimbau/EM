namespace EventsManager.Shared.Dtos;

public class EventPriceDto
{
    public Guid Id { get; set; }
    public decimal? Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid EventId { get; set; }
}   