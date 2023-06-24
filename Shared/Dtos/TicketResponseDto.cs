namespace EventsManager.Shared.Dtos;

public class TicketResponseDto
{
    public string Text { get; set; }
    public DateTime ResponseDate { get; set; }
    public bool IsAdminResponse { get; set; }
    public string RespondedBy { get; set; }
}