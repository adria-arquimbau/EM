namespace EventsManager.Server.Models;

public class TicketResponse
{
    public Guid Id { get; set; }
    public string Text { get; set; }
    public DateTime ResponseDate { get; set; }
    public bool IsAdminResponse { get; set; }
    public ApplicationUser RespondedBy { get; set; }
    public Ticket Ticket { get; set; }

    public TicketResponse()
    {
        ResponseDate = DateTime.UtcNow;
    }
}