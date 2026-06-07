namespace Cirrus.Base.Services.Abstract;

public interface ILocalizationService
{
    string Localize(string key);
}

public static class LocalizationServiceExtensions
{
    public static string LocalizeFormatted(this ILocalizationService localizationService, string formatKey, params object[] args)
    {
        var format = localizationService.Localize(formatKey);
        return string.Format(format, args);
    }
}