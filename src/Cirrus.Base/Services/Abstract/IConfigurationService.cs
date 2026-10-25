using Microsoft.Extensions.Configuration;

namespace Cirrus.Base.Services.Abstract;

public interface IConfigurationService
{
    IConfiguration GetConfiguration();

    static IConfigurationService? GetService() => ServicesProvider.GetService<IConfigurationService>();
}