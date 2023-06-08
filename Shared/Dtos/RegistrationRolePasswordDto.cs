using EventsManager.Shared.Enums;

namespace EventsManager.Shared.Dtos;

public class RegistrationRolePasswordDto
{
    public RegistrationRole Role { get; set; }
    public string? Password { get; set; }
}