//using System.Collections.Frozen; // Optional: Use if .NET 8+, otherwise standard HashSet
using System.Runtime.CompilerServices;

using Linguistics.Data;

namespace Linguistics;

/// <summary>
/// High-performance, zero-allocation Arabic Stop Word checker.
/// Optimized for .NET 6+ using Length-Bucketed Lookups for Spans.
/// </summary>
public static class ArabicStopWords
{
    #region DATA STORAGE

    // ........................................................................
    // DATA STORAGE
    // ........................................................................

    // Primary storage for String inputs (O(1) Lookup)
    private static readonly HashSet<string> stopWordsSet;

    // Secondary storage for Span inputs (Zero-Allocation)
    // Index = Word Length. Value = Array of stop words of that length.
    // Example: _stopWordsByLength[3] contains ["منذ", "على", "إلى"...]
    private static readonly string[][] stopWordsByLength;

    // Maximum length of a stop word to avoid bounds checking issues
    private static readonly int maxWordLength;

    static ArabicStopWords()
    {
        // 1. The Cleaned, Authoritative List
        // Removed: Punctuation, Typos, Duplicates.
        // Kept: Prepositions, Pronouns, Conjunctions, Common Adverbs, Months (as per original intent).
        // 2. Initialize HashSet for O(1) string lookups
        stopWordsSet = ArabicStopWordsData.All;

        // 3. Initialize Length-Bucketed Arrays for Zero-Alloc Span lookups
        if (stopWordsSet.Count > 0)
        {
            maxWordLength = stopWordsSet.Max(w => w.Length);
            stopWordsByLength = new string[maxWordLength + 1][];

            // Group by length
            var grouped = stopWordsSet.GroupBy(w => w.Length);

            foreach (var group in grouped)
            {
                // Convert to array.
                // Optimization: Sorting helps CPU branch prediction slightly during linear scans.
                stopWordsByLength[group.Key] = group.OrderBy(x => x).ToArray();
            }
        }
        else
        {
            stopWordsByLength = Array.Empty<string[]>();
        }
    }

    #endregion DATA STORAGE

    #region PUBLIC API

    // ........................................................................
    // PUBLIC API
    // ........................................................................

    /// <summary>
    /// Checks if the word is a stop word.
    /// Optimized for String inputs (O(1) Lookup).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsStopWord(string word)
    {
        if (string.IsNullOrEmpty(word)) return false;
        return stopWordsSet.Contains(word);
    }

    /// <summary>
    /// Checks if the word is a stop word.
    /// Optimized for Span inputs (Zero-Allocation).
    /// </summary>
    public static bool IsStopWord(ReadOnlySpan<char> wordSpan)
    {
        int len = wordSpan.Length;

        // 1. Bounds Check
        // If the word is too long or empty, it can't be in our list.
        if (len == 0 || len > maxWordLength)
        {
            return false;
        }

        // 2. Get the bucket for this length
        // This array access is safe because of the bounds check above.
        string[]? bucket = stopWordsByLength[len];

        if (bucket == null)
        {
            return false;
        }

        // 3. Linear Scan within the bucket
        // Since buckets are small (e.g., ~30 words), this is faster than allocating a string.
        // We use Span.SequenceEqual which is highly optimized in .NET.
        foreach (string stopWord in bucket)
        {
            if (wordSpan.SequenceEqual(stopWord.AsSpan()))
            {
                return true;
            }
        }

        return false;
    }

    #endregion PUBLIC API
}
