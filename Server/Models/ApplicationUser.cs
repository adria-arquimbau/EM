using Microsoft.AspNetCore.Identity;

namespace EventsManager.Server.Models;

public class ApplicationUser : IdentityUser
{
    public virtual string? Name { get; set; }
    public virtual string? FamilyName { get; set; }
    public virtual string? Country { get; set; }
    public virtual string? City { get; set; }
    public virtual string? PostalCode { get; set; }
    public virtual string? Address { get; set; }
    public virtual string? PhoneNumber { get; set; }    
    public virtual Uri? ImageUrl { get; set; }
    public virtual DateTime? LastLoginTime { get; set; }
    public virtual DateTime? RegistrationDate { get; set; }
    public virtual ICollection<Registration> Registrations { get; set; }
    public virtual List<Event> OwnedEvents { get; set; } = new();
    public virtual List<Event> CreatorEvents { get; set; } = new();
        
    public void UpdateLastLoginTime()   
    {
        LastLoginTime = DateTime.UtcNow;
    }
        
    public void SetRegistrationDate()   
    {
        RegistrationDate = DateTime.UtcNow;
    }
}   
