using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cirrus.Base.Services.Abstract;

public interface IConfigurationService
{
    IConfiguration GetConfiguration();
    
    static IConfigurationService? GetService() =>
        ServicesProvider.Current.GetService<IConfigurationService>();
}