using Microsoft.Extensions.DependencyInjection;

namespace Cirrus.Base.Services.Abstract;

public interface IExceptionHandlingService
{
    protected Task<bool> HandleAsync(string exceptionMessage, string actionCaption, string dismissCaption,
        Func<Task> action);

    protected Task HandleAsync(string exceptionMessage, string dismissCaption);

    static IExceptionHandlingService? GetService() =>
        ServicesProvider.Current.GetService<IExceptionHandlingService>();
}