namespace EventsManager.Shared.Requests;

public class TicketRequest
{
    public string Title { get; set; }
    public string Text { get; set; }
    public bool Solved { get; set; }
}