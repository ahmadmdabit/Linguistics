using Linguistics.Data;

namespace Linguistics;

/// <summary>
/// Provides optimized access to Arabic Definite Articles.
/// Data is sorted by Length Descending to support Greedy Matching.
/// </summary>
public static class ArabicDefiniteArticle
{
    private static readonly string[] articles;

    static ArabicDefiniteArticle()
    {
        // Sort by Length Descending (Greedy Match)
        // "وال" (3) comes before "ال" (2)
        articles = ArabicAffixesData.DefiniteArticles
            .OrderByDescending(s => s.Length)
            .ToArray();
    }

    /// <summary>
    /// Gets the list of definite articles sorted by length (longest first).
    /// Iterating this Span is Zero-Allocation.
    /// </summary>
    public static ReadOnlySpan<string> Articles => articles;
}
