using Cirrus.Base.Primitives;

namespace Cirrus.Network.Exceptions;

public sealed class UnsuccessfulResponseException : Exception, ILoggableException
{
    public required string ApiRoute { get; init; }
    public required int StatusCode { get; init; }
    public required DateTimeOffset ThrownTime { get; init; }

    public (Exception Exception, Dictionary<string, string> Properties) GenerateLog() =>
    (
        new("UnsuccessfulResponse"),
        new()
        {
            [nameof(ApiRoute)] = ApiRoute,
            [nameof(StatusCode)] = StatusCode.ToString(),
            [nameof(ThrownTime)] = ThrownTime.ToString("O")
        }
    );
}