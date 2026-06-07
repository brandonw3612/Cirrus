using System.Collections.ObjectModel;

namespace Cirrus.Playback.Extensions;

/// <summary>
/// Extensions for generic collections.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Get the index of the first element that matches the predicate.
    /// </summary>
    /// <typeparam name="T">Type of elements in the collection.</typeparam>
    /// <param name="collection">The collection.</param>
    /// <param name="predicate">Predicate to match the element.</param>
    /// <param name="startIndex">The index where we begin the search.</param>
    /// <returns>The index of the first element that matches the predicate. -1 if not found.</returns>
    public static int IndexOf<T>(this ObservableCollection<T> collection, Predicate<T> predicate, int startIndex = 0)
    {
        for (var i = startIndex; i < collection.Count; i++)
        {
            if (predicate(collection[i])) return i;
        }
        return -1;
    }
    
    /// <summary>
    /// Get the longest common subsequence between two collections.
    /// </summary>
    /// <param name="a">First collection.</param>
    /// <param name="b">Second collection.</param>
    /// <param name="equalityComparer">Equality comparer of the elements.</param>
    /// <typeparam name="T">Type of elements in the collection.</typeparam>
    /// <returns>The longest common subsequence between the two collections.</returns>
    public static IReadOnlyCollection<T> LongestCommonSubsequenceWith<T>(this IReadOnlyCollection<T> a,
        IReadOnlyCollection<T> b, IEqualityComparer<T> equalityComparer)
    {
        // Reference: https://en.wikipedia.org/wiki/Longest_common_subsequence_problem#Computing_the_length_of_the_LCS
        var lengths = new int[a.Count + 1, b.Count + 1];
        // row 0 and column 0 are initialized to 0 already
        for (var i = 0; i < a.Count; i++)
        {
            for (var j = 0; j < b.Count; j++)
            {
                if (equalityComparer.Equals(a.ElementAt(i), b.ElementAt(j)))
                    lengths[i + 1, j + 1] = lengths[i, j] + 1;
                else
                    lengths[i + 1, j + 1] = Math.Max(lengths[i + 1, j], lengths[i, j + 1]);
            }
        }
        // read the substring out from the matrix
        var result = new List<T>();
        int x = a.Count, y = b.Count;
        while (x is not 0 && y is not 0)
        {
            if (lengths[x, y] == lengths[x - 1, y]) x--;
            else if (lengths[x, y] == lengths[x, y - 1]) y--;
            else
            {
                result.Add(a.ElementAt(x - 1));
                x--;
                y--;
            }
        }
        result.Reverse();
        return result;
    }

    /// <summary>
    /// Rearrange an observable collection to match the target collection.
    /// </summary>
    /// <param name="collection">The observable collection to update.</param>
    /// <param name="targetCollection">The target collection to match.</param>
    /// <param name="equalityComparer">Equality comparer of the elements.</param>
    /// <typeparam name="T">Type of elements in the collection.</typeparam>
    public static void RearrangeWith<T>(this ObservableCollection<T> collection,
        IReadOnlyCollection<T> targetCollection, IEqualityComparer<T> equalityComparer)
    {
        // First, we compute the longest common sequence of the collections.
        // All elements in the LCS are not movable.
        var lcs = collection.LongestCommonSubsequenceWith(targetCollection, equalityComparer).ToList();
        // We indicate the position of the last element in the LCS.
        int pl = -1;
        foreach (var target in targetCollection)
        {
            if (lcs.Contains(target))
            {
                // Current element is in the LCS.
                // We search for current element in the observable collection and update 'pl'.
                do
                {
                    pl++;
                } while (!equalityComparer.Equals(collection[pl], target));
            }
            else
            {
                // Current element is not in the LCS.
                // We search for current element in the observable collection.
                var targetIndex = collection.IndexOf(e => equalityComparer.Equals(e, target));
                // (1) If the target element is right on the position we do not have to move the element.
                //      However, this could not happen because 'pl' indicates the position of the last LCS element.
                //  // if (targetIndex == pl) continue;
                // (2) If the last LCS element is on the left of the target element,
                //      we move the target element to the right of the last LCS element,
                //      position of which would be (pl + 1). 
                if (targetIndex > pl) pl++;
                // (3) Otherwise the target element is on the left.
                //      After it is moved, the position of the last LCS element would be (pl - 1).
                //      Therefore, the position of our element is (pl).
                // (4) Consider (1) again. If the element is right on the target position, then no moving is needed.
                if (targetIndex != pl) collection.Move(targetIndex, pl);
                // After moving the target element, we are sure that the target element should be in the LCS.
                // Yet we do not update the LCS since it is not necessary.
                // Later we will just search for the next element in the LCS, starting right from the target element.
            }
        }
        // The algorithm is above provides the best solution for updating the observable collection.
        // Here is a proof:
        // 1. We proof that each element not in the LCS is moved exactly once.
        //      (1) Let us consider each element in the LCS as separators.
        //          For (n) elements in the LCS, the collection is divided into (n + 1) sections.
        //      (2) The rest of the elements lie in these sections. Yet some of the sections could be empty.
        //          Since these elements are not in the LCS, we are sure that they are not in the target section,
        //          thus need to be moved. (If they are in the target section, they would be in the LCS.)
        //      (3) We can simplify this problem as moving these elements while keeping them in order
        //          so each element only needs one move, just like remove them all at once,
        //          then put them back to the sequence one by one.
        //      (4) Actually in the algorithm, if we ignore those not in the LCS until we move them,
        //          it is exactly the same as the simplified problem.
        // 2. We proof that the algorithm provides the best solution.
        //      (1) From the very beginning, we have the common sequence at its longest length.
        //      (2) As we move the non-LCS elements, the length of the common sequence increases by 1 on each move.
        //      (3) It is obvious that we are not able to extend the LCS by more than 1 on each operation.
        //      (4) Therefore, the algorithm provides the best solution.
    }
}