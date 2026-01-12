namespace SmartFactory.Application.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessException : BaseException
{
    public BusinessException(string message)
        : base(message, 400, "BUSINESS_ERROR")
    {
    }

    public BusinessException(string message, string errorCode)
        : base(message, 400, errorCode)
    {
    }
}
