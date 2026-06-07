using Microsoft.Extensions.DependencyInjection;

namespace Cirrus.Base.Services.Abstract;

public interface IEventLogService
{
    Task LogEventAsync(string eventIdentifier, Dictionary<string, string> context);
    Task LogErrorAsync(Exception exception, Dictionary<string, string> context);
    
    static IEventLogService? GetService() =>
        ServicesProvider.Current.GetService<IEventLogService>();
}