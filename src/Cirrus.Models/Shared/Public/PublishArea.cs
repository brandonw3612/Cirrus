namespace Cirrus.Models.Shared.Public;

/// <summary>
/// Area query parameter in API public/new-tracks, indicating geographical area the query is about.
/// </summary>
public enum PublishArea
{
    /// <summary>
    /// Unspecified area.
    /// </summary>
    Unspecified = 0,
    /// <summary>
    /// Mandarin areas.
    /// </summary>
    Mandarin = 7,
    /// <summary>
    /// Western areas.
    /// </summary>
    Western = 96,
    /// <summary>
    /// Japanese areas.
    /// </summary>
    Japanese = 8,
    /// <summary>
    /// Korean areas.
    /// </summary>
    Korean = 16
}