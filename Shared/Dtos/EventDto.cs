namespace EventsManager.Shared.Dtos;

public class EventDto
{
    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Name { get; set; }    
    public string Description { get; set; }
    public string Location { get; set; }
    public Uri? ImageUrl { get; set; }
    public int PreAndAcceptedRidersRegistrationsCount { get; set; }
    public bool RiderHaveRegistrationRolePassword { get; set; }
    public bool StaffHaveRegistrationRolePassword { get; set; }
    public bool MarshallHaveRegistrationRolePassword { get; set; }
    public bool RiderMarshallHaveRegistrationRolePassword { get; set; } 
    public int MaxRegistrations { get; set; }
    public DateTime OpenRegistrationsDate { get; set; }
    public DateTime CloseRegistrationsDate { get; set; }
    public bool IsFree { get; set; }
    public decimal? CurrentPrice { get; set; }
    public List<EventPriceDto> Prices { get; set; }
}
    