using EventsManager.Shared.Enums;

namespace EventsManager.Shared.Dtos;

public class RegistrationToCheckInDto
{
    public Guid Id { get; set; }
    public DateTime CreationDate { get; set; }
    public RegistrationRole Role { get; set; }
    public RegistrationState State { get; set; }
    public int? Bib { get; set; }
    public bool CheckedIn { get;  set; }
    public UserDto RegisteredUser { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public decimal? AmountPaid { get; set; }
    public bool IsFreeEvent { get; set; }
}