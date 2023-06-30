namespace EventsManager.Server.Models;

public class Payment
{
    public Guid Id { get; set; }
    public DateTime CreationDate { get; private set; }
    public string Type { get; set; }
    public Registration Registration { get; set; }
    
    public Payment()
    {
    }
}
