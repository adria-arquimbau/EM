namespace EventsManager.Shared.Dtos;

public class TicketDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Text { get; set; }
    public bool Solved { get; set; }
    public string? SolvedBy { get; set; }   
    public string CreatedBy { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime? SolvedDate { get; set; }
    public List<TicketResponseDto> TicketResponses { get; set; }
}