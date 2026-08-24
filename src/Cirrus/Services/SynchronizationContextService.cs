using Cirrus.Base.Services.Abstract;

namespace Cirrus.Services;

public class SynchronizationContextService(SynchronizationContext context) : ISynchronizationContextService
{
    public SynchronizationContext Get() => context;
}