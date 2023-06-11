namespace EventsManager.Shared.Dtos;

public class UserDto
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? FamilyName { get; set; }
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public Uri? ImageUrl { get; set; }
    public bool EmailConfirmed { get; set; }    
    public bool RequestingUpdate { get; set; }
    public bool? IsOrganizer { get; set; }
}