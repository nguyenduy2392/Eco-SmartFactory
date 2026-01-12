namespace SmartFactory.Application.Exceptions;

/// <summary>
/// Exception thrown when user does not have permission to access a resource
/// </summary>
public class ForbiddenException : BaseException
{
    public ForbiddenException(string message)
        : base(message, 403, "FORBIDDEN")
    {
    }

    public ForbiddenException()
        : base("You do not have permission to access this resource.", 403, "FORBIDDEN")
    {
    }
}
