using System.ComponentModel.DataAnnotations;

namespace EventsManager.Shared.Dtos;

public class EventPriceDto
{
    public Guid Id { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Price must be greater than or equal to 1")]
    public decimal? Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid EventId { get; set; }
}   