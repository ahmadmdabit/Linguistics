using System.Runtime.CompilerServices;

using Linguistics.Data;

namespace Linguistics;

/// <summary>
/// High-performance, zero-allocation checker for Strange/Foreign words.
/// Optimized using Length-Bucketed Lookups for Spans.
/// </summary>
public static class ArabicStrange
{
    // Primary storage for String inputs (O(1) Lookup)
    private static readonly HashSet<string> strangeSet;

    // Secondary storage for Span inputs (Zero-Allocation)
    // Index = Word Length. Value = Array of words of that length.
    private static readonly string[][] strangeByLength;

    private static readonly int maxWordLength;

    static ArabicStrange()
    {
        var rawList = ArabicStrangeData.All;

        // 1. Initialize HashSet
        strangeSet = new HashSet<string>(rawList, StringComparer.Ordinal);

        // 2. Initialize Length-Bucketed Arrays
        if (rawList.Count > 0)
        {
            maxWordLength = rawList.Max(w => w.Length);
            strangeByLength = new string[maxWordLength + 1][];

            var grouped = rawList.GroupBy(w => w.Length);
            foreach (var group in grouped)
            {
                strangeByLength[group.Key] = group.OrderBy(x => x).ToArray();
            }
        }
        else
        {
            strangeByLength = Array.Empty<string[]>();
        }
    }

    /// <summary>
    /// Checks if the word is a known strange/foreign word.
    /// Optimized for Span inputs (Zero-Allocation).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsStrangeWord(ReadOnlySpan<char> wordSpan)
    {
        int len = wordSpan.Length;
        if (len == 0 || len > maxWordLength) return false;

        string[]? bucket = strangeByLength[len];
        if (bucket == null) return false;

        foreach (string strangeWord in bucket)
        {
            if (wordSpan.SequenceEqual(strangeWord.AsSpan()))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the word is a known strange/foreign word.
    /// Optimized for String inputs.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsStrangeWord(string word)
    {
        if (string.IsNullOrEmpty(word)) return false;
        return strangeSet.Contains(word);
    }
}
