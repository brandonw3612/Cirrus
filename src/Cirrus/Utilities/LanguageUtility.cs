namespace Cirrus.Utilities;

public static class LanguageUtility
{
    public static string GetSupportedSystemLanguage(string? preferredLanguage)
    {
        Dictionary<string, string[]> supportedLanguages = new()
        {
            ["en-US"] = new[]
            {
                "en", "en-au", "en-ca", "en-gb", "en-ie", "en-in", "en-nz", "en-sg", "en-us", "en-za",
                "en-bz", "en-hk", "en-id", "en-jm", "en-kz", "en-mt", "en-my", "en-ph", "en-pk", "en-tt",
                "en-vn", "en-zw", "en-053", "en-021", "en-029", "en-011", "en-018", "en-014"
            },
            ["zh-Hans"] = new[]
            {
                "zh-hans", "zh-cn", "zh-hans-cn", "zh-sg", "zh-hans-sg"
            },
            ["zh-Hant"] = new[]
            {
                "zh-hant", "zh-hk", "zh-mo", "zh-tw", "zh-hant-hk", "zh-hant-mo", "zh-hant-tw"
            }
        };
        if (preferredLanguage is not null && supportedLanguages.ContainsKey(preferredLanguage)) return preferredLanguage;
        var userProfileLanguages = Windows.System.UserProfile.GlobalizationPreferences.Languages;
        foreach (var lang in userProfileLanguages)
        {
            foreach (var supportedLang in supportedLanguages)
            {
                if (supportedLang.Value.Contains(lang.ToLower()))
                    return supportedLang.Key;
            }
        }
        return "en-US";
    }
}