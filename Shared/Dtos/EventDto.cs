namespace EventsManager.Shared.Dtos;

public class EventDto
{
    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    public string Name { get; set; }    
    public string Description { get; set; }
    public string Location { get; set; }
    public Uri? ImageUrl { get; set; }
    
    public int PreRegistrationsCount { get; set; }
}
