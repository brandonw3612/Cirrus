using Cirrus.Models.Business.Developer;

namespace Cirrus.Base.Services.Abstract;

public interface ISoftwareUpdateService
{
    IAsyncEnumerable<ISoftwareUpdate> FetchUpdatesAsync(bool includePrerelease = false);
    
    static ISoftwareUpdateService? GetService() => ServicesProvider.GetService<ISoftwareUpdateService>();
}