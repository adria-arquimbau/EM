namespace EventsManager.Shared.Exceptions;

public class UserNotFoundException : Exception
{
    public UserNotFoundException(string value) : base(value)
    {
    }
}