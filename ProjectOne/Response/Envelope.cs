using ProjectOne.Domain.Common;

namespace ProjectOne.Response;

public record Envelope
{
    public object? Result { get; }
    
    public string? StatusCode { get; }
    
    public string? Message { get; }
    
    public DateTime Timestamp { get; }

    private Envelope(object? result, Error? error)
    {
        Result = result;
        StatusCode = error?.Code;
        Message = error?.Message;
        Timestamp = DateTime.UtcNow;
    }
    
    public static Envelope Ok(object? result)
        => new(result, null);
    
    public static Envelope Error(Error? error)
        => new(null, error);
}