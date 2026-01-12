namespace SmartFactory.Application.Exceptions;

/// <summary>
/// Exception thrown when user is not authorized to perform an action
/// </summary>
public class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message)
        : base(message, 401, "UNAUTHORIZED")
    {
    }

    public UnauthorizedException()
        : base("You are not authorized to perform this action.", 401, "UNAUTHORIZED")
    {
    }
}
