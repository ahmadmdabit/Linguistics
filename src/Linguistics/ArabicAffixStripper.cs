using Linguistics.Data;

namespace Linguistics;

/// <summary>
/// Responsible for stripping affixes (prefixes, suffixes, definite articles) from Arabic words.
/// </summary>
public static class ArabicAffixStripper
{
    /// <summary>
    /// Checks for and removes definite articles (e.g., "ال", "وال").
    /// </summary>
    public static void CheckDefiniteArticle(ref MorphologyResult result)
    {
        ReadOnlySpan<char> current = result.Text;

        foreach (string article in ArabicDefiniteArticle.Articles)
        {
            ReadOnlySpan<char> articleSpan = article.AsSpan();

            if (current.StartsWith(articleSpan))
            {
                ReadOnlySpan<char> candidate = current.Slice(articleSpan.Length);

                if (candidate.Length < 2) continue;

                // Recursive Step using ArabicMorphologyHelper to close the loop
                // Note: We need to call back to the main pipeline to analyze the stripped candidate.
                // Since this is a static helper, we can't easily inject the "Analyzer".
                // For now, we will replicate the AnalyzeCandidate logic or call a delegate if we want true decoupling.
                // However, to keep it simple and consistent with the previous design, we will expose a way to analyze the candidate.

                // Refactoring Note: The original code called AnalyzeCandidate recursively.
                // To avoid circular dependency hell or complex delegates in this static class,
                // we will use ArabicMorphologyHelper.AnalyzeCandidate directly if we make it internal/public,
                // OR we move AnalyzeCandidate to a shared location.

                // Better approach for SOLID: This class should just STRIP and return the candidate, 
                // but the recursive nature ("Strip -> Analyze -> if valid, stop") makes it hard.

                // Pragmatic Solution for now: Call ArabicMorphologyHelper.AnalyzeCandidate.
                // We will need to make ArabicMorphologyHelper.AnalyzeCandidate internal or public.
                // Since we are refactoring, let's assume ArabicMorphologyHelper will have a public/internal AnalyzeCandidate.

                ArabicMorphologyHelper.AnalyzeCandidate(ref result, candidate);

                if (result.IsFinished) return;

                // Fallback Logic
                if (candidate.Length > 3)
                {
                    candidate.CopyTo(result.RawBuffer);
                    result.UpdateLength(candidate.Length);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Checks for and removes prefixes (e.g., "ل", "ب").
    /// </summary>
    public static void CheckPrefixes(ref MorphologyResult result)
    {
        ReadOnlySpan<char> current = result.Text;

        foreach (string prefix in ArabicPrefixes.Prefixes)
        {
            ReadOnlySpan<char> prefixSpan = prefix.AsSpan();

            if (current.StartsWith(prefixSpan))
            {
                ReadOnlySpan<char> candidate = current.Slice(prefixSpan.Length);

                if (candidate.Length < 2) continue;

                ArabicMorphologyHelper.AnalyzeCandidate(ref result, candidate);

                if (result.IsFinished) return;
            }
        }
    }

    /// <summary>
    /// Checks for and removes suffixes (e.g., "هم", "ات").
    /// </summary>
    public static void CheckSuffixes(ref MorphologyResult result)
    {
        result.IsProcessingSuffixes = true;

        ReadOnlySpan<char> current = result.Text;

        foreach (string suffix in ArabicSuffixes.Suffixes)
        {
            ReadOnlySpan<char> suffixSpan = suffix.AsSpan();

            if (current.EndsWith(suffixSpan))
            {
                ReadOnlySpan<char> candidate = current.Slice(0, current.Length - suffixSpan.Length);

                if (candidate.Length < 2) continue;

                ArabicMorphologyHelper.AnalyzeCandidate(ref result, candidate);

                if (result.IsFinished)
                {
                    result.IsProcessingSuffixes = false;
                    return;
                }
            }
        }

        result.IsProcessingSuffixes = false;
    }

    public static void CheckPrefixWaw(ref MorphologyResult result)
    {
        ReadOnlySpan<char> current = result.Text;
        if (current.Length > 3 && current[0] == ArabicConstantsChar.Waw)
        {
            ArabicMorphologyHelper.AnalyzeCandidate(ref result, current.Slice(1));
        }
    }
}
