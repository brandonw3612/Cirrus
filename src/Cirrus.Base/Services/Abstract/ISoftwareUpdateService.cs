using Cirrus.Models.Business.Developer;
using Microsoft.Extensions.DependencyInjection;

namespace Cirrus.Base.Services.Abstract;

public interface ISoftwareUpdateService
{
    IAsyncEnumerable<ISoftwareUpdate> FetchUpdatesAsync(bool includePrerelease = false);
    
    static ISoftwareUpdateService? GetService() =>
        ServicesProvider.Current.GetService<ISoftwareUpdateService>();
}