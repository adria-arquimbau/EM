using EventsManager.Server.Controllers;
using EventsManager.Shared.Enums;

namespace EventsManager.Server.Models;

public class Registration
{
    public Guid Id { get; set; }
    public DateTime CreationDate { get; private set; }
    public RegistrationRole Role { get; private set; }
    public RegistrationState State { get; set; }
    public int? Bib { get; private set; }
    public bool CheckedIn { get; private set; }
    public Event Event { get; set; }
    public string UserId { get; set; }
    public virtual ApplicationUser User { get; set; }
    public PaymentStatus PaymentStatus { get; set; }

    private Registration()  
    {
    }

    public Registration(ApplicationUser user, RegistrationRole role, RegistrationState state, Event sportEvent)
    {
        CreationDate = DateTime.UtcNow;
        User = user;
        Role = role;
        State = state;
        Event = sportEvent;
        PaymentStatus = PaymentStatus.UnPaid;
    }

    public void Update(int? bib, bool checkedIn, RegistrationState state)
    {
        Bib = bib;
        CheckedIn = checkedIn;
        State = state;
    }
}
