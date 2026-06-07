using Cirrus.Base.Services.Abstract;
using CommunityToolkit.WinUI;

namespace Cirrus.Services;

public class LocalizationService : ILocalizationService
{
    public string Localize(string key) => key.GetLocalized() ?? "{Invalid Resource}";
}