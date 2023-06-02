namespace EventsManager.Server.Models;

public class Event
{
    public Guid Id { get; private set; }
    public DateTime CreationDate { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime FinishDate { get; private set; }
    public ApplicationUser? Owner { get; private set;  }
    public string Name { get; private set; }    
    public string Description { get; private set; }
    public string Location { get; private set; }
    public Uri? ImageUrl { get; private set; }   
    public int MaxRegistrations { get; private set; }   
    public bool IsPublic { get; private set; }  
    public DateTime OpenRegistrationsDate { get; private set; }  
    public DateTime CloseRegistrationsDate { get; private set; }
    
    private Event() { }
    
    public Event(string name, string description, string location, int maxRegistrations, ApplicationUser owner, DateTime startDate, DateTime finishDate, DateTime openRegistrationsDate, DateTime closeRegistrationsDate)
    {
        Owner = owner;
        Name = name;    
        Description = description;    
        CreationDate = DateTime.UtcNow;
        Location = location;
        MaxRegistrations = maxRegistrations;
        StartDate = startDate;
        FinishDate = finishDate;
        IsPublic = false;
        OpenRegistrationsDate = openRegistrationsDate;
        CloseRegistrationsDate = closeRegistrationsDate;
    }
}
