namespace Cirrus.Base.Exceptions;

/// <summary>
/// Exception thrown as the user tries to add new services to the IoC container
/// while the global service provider is already built. 
/// </summary>
public class ServiceProviderAlreadyBuiltException : Exception
{
    /// <summary>
    /// Constructs a new <see cref="ServiceProviderAlreadyBuiltException"/>.
    /// </summary>
    /// <param name="message">Message of the exception.</param>
    public ServiceProviderAlreadyBuiltException(string message) : base(message)
    {
        // No further action.
    }
}