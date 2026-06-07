using Cirrus.Base.Primitives;

namespace Cirrus.Network.Exceptions;

public sealed class DeserializationFailedException : Exception, ILoggableException
{
    public required string ApiRoute { get; init; }
    public required string PropertyPath { get; init; }
    public required DateTimeOffset ThrownTime { get; init; }
    
    public DeserializationFailedException(Exception? innerException) : base(null, innerException)
    {
        // No further actions.
    }

    public (Exception Exception, Dictionary<string, string> Properties) GenerateLog() =>
    (
        InnerException ?? new Exception("DeserializationFailed"),
        new()
        {
            [nameof(ApiRoute)] = ApiRoute,
            [nameof(PropertyPath)] = PropertyPath,
            [nameof(ThrownTime)] = ThrownTime.ToString("O"),
        }
    );
}