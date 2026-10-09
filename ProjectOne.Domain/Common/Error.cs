namespace ProjectOne.Domain.Common;

public record Error
{
    public string Code { get; }
    public string Message { get; }
    public ErrorType ErrorType { get; }

    private Error(string code, string message, ErrorType errorType)
    {
        Code = code;
        Message = message;
        ErrorType = errorType;
    }
    
    public static Error Validation(string code, string message)
        => new(code, message, ErrorType.Validation);
    
    public static Error NotFound(string code, string message)
        => new(code, message, ErrorType.NotFound);
    
    public static Error Failure(string code, string message)
        => new(code, message, ErrorType.Failure);
    
    public static Error Conflict(string code, string message)
        => new(code, message, ErrorType.Conflict);

    public static Error Forbidden(string code, string message)
        => new(code, message, ErrorType.Forbidden);
}

public enum ErrorType
{
    Validation,
    NotFound,
    Failure,
    Conflict,
    Forbidden
}