using Microsoft.AspNetCore.Identity;

namespace EventsManager.Server.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual string? Name { get; set; }
        public virtual string? FamilyName { get; set; }
        public virtual string? Country { get; set; }
        public virtual string? City { get; set; }
        public virtual string? PostalCode { get; set; }
        public virtual string? Address { get; set; }
        public virtual string? PhoneNumber { get; set; }
        public virtual DateTime? LastLoginTime { get; set; }
        public virtual DateTime? RegistrationDate { get; set; }
        
        public void UpdateLastLoginTime()   
        {
            LastLoginTime = DateTime.Now;
        }
        
        public void SetRegistrationDate()   
        {
            RegistrationDate = DateTime.Now;
        }
    }
}