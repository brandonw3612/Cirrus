using System.Collections.ObjectModel;

namespace Cirrus.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> TakeAtMost<T>(this IEnumerable<T> source, int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (count == 0) yield break;
        using var enumerator = source.GetEnumerator();
        for (var i = 0; i < count && enumerator.MoveNext(); i++)
            yield return enumerator.Current;
    }

    public static bool IsEquivalentWith<T>(this IEnumerable<T> sequence, IEnumerable<T> another)
    {
        var a = sequence.ToHashSet();
        var b = another.ToHashSet();
        a.SymmetricExceptWith(b);
        return a.Count == 0;
    }

    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source) => new(source);
}