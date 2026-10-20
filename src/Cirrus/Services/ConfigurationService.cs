using Cirrus.Base.Services.Abstract;
using Microsoft.Extensions.Configuration;

namespace Cirrus.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configurationRoot = BuildConfiguration();

    public IConfiguration GetConfiguration() => _configurationRoot;

    private static IConfiguration BuildConfiguration()
    {
        var builder = new ConfigurationBuilder();
        using var stream = typeof(ConfigurationService).Assembly.GetManifestResourceStream(ConfigResourceName);
        if (stream is not null)
        {
            builder.AddJsonStream(stream);
        }
        return builder.Build();
    }

#if PRODUCTION
    private const string ConfigResourceName = "Cirrus.app.config.json";
#else
    private const string ConfigResourceName = "Cirrus.app.dev.config.json";
#endif
}
