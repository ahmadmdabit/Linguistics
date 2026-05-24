using Linguistics.Data;

namespace Linguistics;

/// <summary>
/// Provides optimized access to Arabic Suffixes.
/// Data is sorted by Length Descending to support Greedy Matching.
/// </summary>
public static class ArabicSuffixes
{
    private static readonly string[] suffixes;

    static ArabicSuffixes()
    {
        // Sort by Length Descending
        // e.g., "هما" (3) comes before "ها" (2)
        suffixes = ArabicAffixesData.Suffixes
            .OrderByDescending(s => s.Length)
            .ToArray();
    }

    /// <summary>
    /// Gets the list of suffixes sorted by length (longest first).
    /// Iterating this Span is Zero-Allocation.
    /// </summary>
    public static ReadOnlySpan<string> Suffixes => suffixes;
}
