using EventsManager.Shared.Enums;

namespace EventsManager.Shared.Requests;

public class RegistrationUpdateRequest
{
    public Guid Id { get; set; }
    public RegistrationState State { get; set; }
    public int? Bib { get; set; }
    public bool CheckedIn { get;  set; }
}