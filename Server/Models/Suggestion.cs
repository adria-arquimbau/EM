namespace EventsManager.Server.Models;

public class Suggestion
{
    public Guid Id { get; private set; }
    public string Content { get; private set; }
    public DateTime CreatedOn { get; private set; }
    public ApplicationUser User { get; private set; }

    private Suggestion() { }
    public Suggestion(string content, ApplicationUser user)
    {
        Content = content;
        User = user;
        CreatedOn = DateTime.UtcNow;
    }
}
