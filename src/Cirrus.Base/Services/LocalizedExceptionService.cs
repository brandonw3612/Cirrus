using Cirrus.Base.Services.Abstract;

namespace Cirrus.Base.Services;

[RegisterSingleton]
public class LocalizedExceptionService(ILocalizationService localizationService)
{
    public Exception CreateLocalized(string exceptionMessageKey) =>
        new(localizationService.Localize(exceptionMessageKey));

    public Exception CreateLocalized(Exception innerException, string exceptionMessageKey) =>
        new(localizationService.Localize(exceptionMessageKey), innerException);

    public Exception CreateLocalized(string messageFormatKey, params object[] args) =>
        new(localizationService.LocalizeFormatted(messageFormatKey, args));

    public Exception CreateLocalized(Exception innerException, string messageFormatKey, params object[] args) =>
        new(localizationService.LocalizeFormatted(messageFormatKey, args), innerException);
}