using Cirrus.Base.Exceptions;

namespace Cirrus.Base.Services.Abstract;

/// <summary>
/// Abstract service for accessing user preferences. Expected to be implemented by platform-specific projects.
/// </summary>
public interface IPreferenceAccessService
{
    /// <summary>
    /// Tries to get the value of the preference at the specified path.
    /// </summary>
    /// <param name="preferencePath">Path of the preference.</param>
    /// <param name="value">Value of the preference.</param>
    /// <param name="isLarge">Whether the entry contains large content that should be stored in external storage.</param>
    /// <typeparam name="T">Type of the preference value.</typeparam>
    /// <returns>True if the preference exists, false otherwise.</returns>
    bool TryGetValue<T>(string preferencePath, out T? value, bool isLarge = false);
    
    /// <summary>
    /// Sets the value of the preference at the specified path.
    /// </summary>
    /// <param name="preferencePath">Path of the preference.</param>
    /// <param name="value">Value of the preference.</param>
    /// <param name="isLarge">Whether the entry contains large content that should be stored in external storage.</param>
    /// <typeparam name="T">Type of the preference value.</typeparam>
    void SetValue<T>(string preferencePath, T? value, bool isLarge = false);
    
    /// <summary>
    /// Removes the preference at the specified path.
    /// </summary>
    /// <param name="preferencePath">Path of the preference.</param>
    /// <param name="isLarge">Whether the entry contains large content that should be stored in external storage.</param>
    void RemoveValue(string preferencePath, bool isLarge = false);
}

public static class PreferenceAccessServiceExtensions
{
    /// <summary>
    /// Gets the value of the preference at the specified path.
    /// </summary>
    /// <param name="service">Service instance.</param>
    /// <param name="preferencePath">Path of the preference.</param>
    /// <param name="isLarge">Whether the entry contains large content that should be stored in external storage.</param>
    /// <typeparam name="T">Type of the preference value.</typeparam>
    /// <returns>Value of the preference.</returns>
    static T? GetValue<T>(this IPreferenceAccessService service, string preferencePath, bool isLarge = false)
    {
        if (service.TryGetValue(preferencePath, out T? value, isLarge))
            return value;
        throw new PreferenceFetchFailedException(preferencePath);
    }
}