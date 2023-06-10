namespace EventsManager.Server.Models;

public class Event
{
    public Guid Id { get; private set; }
    public DateTime CreationDate { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime FinishDate { get; private set; }
    public ICollection<ApplicationUser> Owners { get; private set;  } = new List<ApplicationUser>();
    public string Name { get; private set; }
    public string Description { get; private set; } 
    public string Location { get; private set; }
    public Uri? ImageUrl { get; set; }      
    public int MaxRegistrations { get; private set; }       
    public bool IsPublic { get; private set; } 
    public bool IsDeleted { get; set; }     
    public DateTime OpenRegistrationsDate { get; private set; }
    public DateTime CloseRegistrationsDate { get; private set; }
    public virtual ICollection<Registration> Registrations { get; set; }
    public ICollection<RegistrationRolePassword> RegistrationRolePasswords { get; set; } = new List<RegistrationRolePassword>();
    public ICollection<EventPrice> Prices { get; set; } = new List<EventPrice>();
    private Event() { }
    
    public Event(string name, string description, string location, int maxRegistrations, ApplicationUser owner, DateTime startDate, DateTime finishDate, DateTime openRegistrationsDate, DateTime closeRegistrationsDate)
    {
        Owners.Add(owner);
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

    public void Update(string name, string description, string location, int maxRegistrations, DateTime startDate, DateTime finishDate, DateTime openRegistrationsDate, DateTime closeRegistrationsDate, bool isPublic)
    {
        Name = name;    
        Description = description;    
        CreationDate = DateTime.UtcNow;
        Location = location;
        MaxRegistrations = maxRegistrations;
        StartDate = startDate;
        FinishDate = finishDate;
        IsPublic = isPublic;
        OpenRegistrationsDate = openRegistrationsDate;
        CloseRegistrationsDate = closeRegistrationsDate;
    }
}