namespace EventsManager.Shared.Dtos;

public class SuggestionDto
{
    public string Content { get; set; }
    public DateTime CreatedOn { get; set; }
    public UserDto User { get;  set; }
}