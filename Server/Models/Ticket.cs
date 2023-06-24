namespace EventsManager.Server.Models;

public class Ticket
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Text { get; set; }
    public bool Solved { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime? SolvedDate { get; set; }
    public ApplicationUser SolvedBy { get; set; }
    public Registration Registration { get; set; }
    public List<TicketResponse> Responses { get; set; } = new List<TicketResponse>();
    
    public Ticket(string title, string text, DateTime creationDate)
    {
        Title = title;
        Text = text;
        CreationDate = creationDate;
    }
}