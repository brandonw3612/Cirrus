using Cirrus.Base.Services.Abstract;
using Microsoft.Extensions.Configuration;

namespace Cirrus.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configurationRoot = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile(ConfigFileName, optional: false, reloadOnChange: true)
        .Build();

    public IConfiguration GetConfiguration() => _configurationRoot;

#if PRODUCTION
    private static readonly string ConfigFileName = "app.config.json";
#else
    private static readonly string ConfigFileName = "app.dev.config.json";
#endif
}