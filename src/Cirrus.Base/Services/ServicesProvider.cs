using Cirrus.Base.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Cirrus.Base.Services;

/// <summary>
/// Service provider for the current application using dependency injection.
/// </summary>
public class ServicesProvider : IServiceProvider
{
    private static ServicesProvider? _current;

    /// <summary>
    /// Global singleton of <see cref="ServicesProvider"/> object.
    /// </summary>
    public static ServicesProvider Current => _current ??= new();

    private readonly IServiceCollection _collection;
    private ServiceProvider? _serviceProvider;

    private ServicesProvider()
    {
        _collection = new ServiceCollection();
        _collection.AddInfrastructureServices();
    }

    /// <summary>
    /// Registers services to the global service provider. Only available before the first use of the provider.
    /// </summary>
    /// <param name="registerAction">Action to register services.</param>
    /// <exception cref="ServiceProviderAlreadyBuiltException">Thrown when the provider is already built.</exception>
    public static void RegisterServices(Action<IServiceCollection> registerAction)
    {
        if (Current._serviceProvider is not null)
        {
            throw new ServiceProviderAlreadyBuiltException(
                "No more services can be registered since the first use of the provider.");
        }
        registerAction.Invoke(Current._collection);
    }
    
    /// <summary>
    /// Tries to register services to the global service provider. Only available before the first use of the provider.
    /// </summary>
    /// <param name="registerAction">Action to register services.</param>
    /// <returns>Whether the registration is successful.</returns>
    public static bool TryRegisterServices(Action<IServiceCollection> registerAction)
    {
        if (Current._serviceProvider is not null) return false;
        registerAction.Invoke(Current._collection);
        return true;
    }

    /// <summary>
    /// Gets the service instance from the global service provider.
    /// </summary>
    /// <param name="serviceType">Type of the requested service.</param>
    /// <returns>Instance of the requested service.</returns>
    public object? GetService(Type serviceType)
    {
        // If the service provider is not built, build it.
        // Therefore, we do not need to manually build the service provider.
        _serviceProvider ??= _collection.BuildServiceProvider();
        return _serviceProvider.GetService(serviceType);
    }
    
    public static T? GetService<T>() where T : class => Current.GetService<T>();
}