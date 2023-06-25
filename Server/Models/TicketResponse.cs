namespace EventsManager.Server.Models;

public class TicketResponse
{
    public Guid Id { get; }
    public string Text { get; set; }
    public DateTime ResponseDate { get; set; }
    public bool IsAdminResponse { get; set; }
    public ApplicationUser RespondedBy { get; set; }
    public Ticket Ticket { get; set; }

    private TicketResponse() { }
    public TicketResponse(string text, ApplicationUser user, bool isAdminResponse)
    {
        Text = text;
        RespondedBy = user;
        IsAdminResponse = isAdminResponse;
        ResponseDate = DateTime.UtcNow;
    }
}