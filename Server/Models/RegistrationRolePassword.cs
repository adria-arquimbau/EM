using EventsManager.Shared.Enums;

namespace EventsManager.Server.Models;

public class RegistrationRolePassword
{
    public Guid Id { get; private set; }
    public RegistrationRole Role { get; set; }
    public string? Password { get; set; }
    public Event Event { get; set; }
}
    