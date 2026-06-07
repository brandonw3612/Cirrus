namespace Cirrus.Base.Exceptions;

public class PreferenceFetchFailedException(string preferencePath)
    : Exception($"Failed to fetch preference at path \"{preferencePath}\".")
{
    public string PreferencePath { get; set; } = preferencePath;
}