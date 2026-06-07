namespace Cirrus.Network.Diagnostics;

/// <summary>
/// Service requested by networking module, used to generate reports on deserialization coverage analysis.
/// </summary>
public interface IDeserializationCoverageService
{
    /// <summary>
    /// Processes an serialized entity by comparing defined type with its raw JSON.
    /// Route is used to identify the API endpoint.
    /// </summary>
    /// <param name="apiRoute">Route of the API endpoint.</param>
    /// <param name="entityType">Type of the deserialized entity.</param>
    /// <param name="rawJson">Raw JSON of the entity.</param>
    void ProcessEntity(string apiRoute, Type entityType, string rawJson);
    
    /// <summary>
    /// Gathers all coverage analysis results and exports them to the specified target.
    /// </summary>
    void ExportReport();
}