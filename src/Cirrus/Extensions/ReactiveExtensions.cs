using System.Reactive.Linq;
using Microsoft.UI.Dispatching;

namespace Cirrus.Extensions;

public static class ReactiveExtensions
{
    public static IObservable<T> ObserveOn<T>(this IObservable<T> source, DispatcherQueue dispatcherQueue)
    {
        return Observable.Create<T>(o =>
        {
            return source.Subscribe(
                value => dispatcherQueue.TryEnqueue(() => o.OnNext(value)),
                error => dispatcherQueue.TryEnqueue(() => o.OnError(error)),
                () => dispatcherQueue.TryEnqueue(o.OnCompleted)
            );
        });
    }
}