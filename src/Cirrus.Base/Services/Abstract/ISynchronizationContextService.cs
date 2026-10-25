namespace Cirrus.Base.Services.Abstract;

public interface ISynchronizationContextService
{
    SynchronizationContext Get();
    
    static ISynchronizationContextService? GetService() => ServicesProvider.GetService<ISynchronizationContextService>();
}