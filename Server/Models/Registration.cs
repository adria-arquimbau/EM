namespace EventsManager.Server.Models;

public class Registration
{
    public Guid Id { get; set; }
    public DateTime CreationDate { get; private set; }
    public RegistrationRole Role { get; private set; }
    public RegistrationState State { get; private set; }
    public int? Bib { get; private set; }
    public bool CheckedIn { get; private set; }
    public Guid EventId { get; set; }
    public Event Event { get; set; }
    public string RegisteredUserId { get; set; }
    public ApplicationUser RegisteredUser { get; set; }
    
    public Registration()
    {
    }
    
    public Registration(string registeredUserId, RegistrationRole role, RegistrationState state, Guid eventId)
    {
        CreationDate = DateTime.UtcNow;
        RegisteredUserId = registeredUserId;
        Role = role;
        State = state;  
        EventId = eventId;
    }
}

public enum RegistrationRole
{   
    Staff,
    Rider,
    Marshal,    
    RiderMarshal
}

public enum RegistrationState
{
    PreRegistered,
    Accepted,
    Cancelled
}
