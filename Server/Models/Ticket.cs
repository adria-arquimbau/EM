namespace EventsManager.Server.Models;

public class Ticket
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Text { get; set; }
    public bool Solved { get; set; }
    public Registration Registration { get; set; }
    
    public Ticket(string title, string text)
    {
        Title = title;
        Text = text;
    }
}
