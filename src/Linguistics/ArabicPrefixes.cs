using Linguistics.Data;

namespace Linguistics;

/// <summary>
/// Provides optimized access to Arabic Prefixes.
/// Data is sorted by Length Descending to support Greedy Matching.
/// </summary>
public static class ArabicPrefixes
{
    private static readonly string[] prefixes;

    static ArabicPrefixes()
    {
        // Sort by Length Descending
        // e.g., "لل" (2) comes before "ل" (1)
        prefixes = ArabicAffixesData.Prefixes
            .OrderByDescending(s => s.Length)
            .ToArray();
    }

    /// <summary>
    /// Gets the list of prefixes sorted by length (longest first).
    /// Iterating this Span is Zero-Allocation.
    /// </summary>
    public static ReadOnlySpan<string> Prefixes => prefixes;
}
