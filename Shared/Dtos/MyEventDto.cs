namespace EventsManager.Shared.Dtos;

public class MyEventDto
{
    public Guid Id { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime FinishDate { get; set; }
    public string Name { get; set; }    
    public string Description { get; set; }
    public string Location { get; set; }
    public Uri? ImageUrl { get; set; }   
    public int MaxRegistrations { get; set; }       
    public bool IsPublic { get; set; }  
    public int RidersPreRegistrationsCount { get; set; }
    public int RidersAcceptedRegistrationsCount { get; set; }   
    public int MarshallAcceptedRegistrationsCount { get; set; }   
    public int RiderMarshallAcceptedRegistrationsCount { get; set; }   
    public DateTime OpenRegistrationsDate { get; set; }  
    public DateTime CloseRegistrationsDate { get; set; }
    public string? StaffRegistrationPassword { get; set; }
    public string? RiderRegistrationPassword { get; set; }
    public string? MarshallRegistrationPassword { get; set; }
    public string? RiderMarshallRegistrationPassword { get; set; }
    public List<EventPriceDto> Prices { get; set; }
    public bool IsFree { get; set; }
    public string? ProductId { get; set; }  
}   
