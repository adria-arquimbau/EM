namespace EventsManager.Shared.Requests;

public class CreateEventRequest
{
    public DateTime StartDate { get; set; }
    public DateTime FinishDate { get; set; }
    public string Name { get; set; }    
    public string Description { get; set; }
    public string Location { get; set; }
    public int MaxRegistrations { get; set; }
    public bool IsFree { get; set; }
    public int FirstPrice { get; set; }
    public DateTime OpenRegistrationsDate { get; set; }  
    public DateTime CloseRegistrationsDate { get; set; }
}