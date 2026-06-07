namespace Cirrus.Utilities.Primitives;

public class AuthenticationResult
{
    public bool Succeeded { get; }
    public Exception? Exception { get; }

    private AuthenticationResult(bool isSucceeded, Exception? exception)
    {
        Succeeded = isSucceeded;
        Exception = exception;
    }

    public static AuthenticationResult CreateSuccess() => new(true, null);
    
    public static AuthenticationResult CreateFailure(Exception exception) => new(false, exception);
}