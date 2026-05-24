using Linguistics.Data;

namespace Linguistics;

/// <summary>
/// The core engine for Arabic morphological analysis and root extraction.
/// Uses a zero-allocation pipeline with Span-based processing.
/// Refactored to use SOLID principles with ArabicRootExtractor and ArabicAffixStripper.
/// </summary>
public static class ArabicMorphologyHelper
{
    // Max buffer size for a single word. 64 chars is plenty for Arabic.
    private const int MaxWordLength = 64;

    /// <summary>
    /// Processes an Arabic word to extract its root or stem.
    /// </summary>
    /// <param name="word">The input word.</param>
    /// <param name="applyFuzzyNormalization">Whether to apply fuzzy matching for orthographic variations.</param>
    /// <returns>The processed root or stem.</returns>
    public static string FormatWord(string word, bool applyFuzzyNormalization)
    {
        if (string.IsNullOrWhiteSpace(word)) return string.Empty;

        // 1. Allocate Stack Memory (Zero GC)
        Span<char> buffer = stackalloc char[MaxWordLength];

        // 2. Initialize the Ref Struct
        var result = new MorphologyResult(buffer, word.AsSpan(), applyFuzzyNormalization);

        // 3. Cleaning Phase
        RemoveDiacritics(ref result);
        RemovePunctuation(ref result);
        RemoveNonLetter(ref result);

        // 4. Analysis Phase
        if (!CheckStrangeWords(ref result))
        {
            if (!CheckStopWords(ref result))
            {
                StemmingWord(ref result);
            }
        }

        // 5. Materialize to String
        return result.Text.ToString();
    }

    /// <summary>
    /// Processes an Arabic word to extract its root or stem (zero-allocation version).
    /// </summary>
    public static int FormatWord(ReadOnlySpan<char> inputWord, Span<char> outputBuffer, bool applyFuzzyNormalization)
    {
        if (inputWord.IsEmpty || inputWord.IsWhiteSpace())
            return 0;

        if (outputBuffer.Length < MaxWordLength)
            return 0;

        Span<char> workBuffer = stackalloc char[MaxWordLength];
        var result = new MorphologyResult(workBuffer, inputWord, applyFuzzyNormalization);

        RemoveDiacritics(ref result);
        RemovePunctuation(ref result);
        RemoveNonLetter(ref result);

        if (!CheckStrangeWords(ref result))
        {
            if (!CheckStopWords(ref result))
            {
                StemmingWord(ref result);
            }
        }

        int resultLength = result.Text.Length;
        if (resultLength > 0 && resultLength <= outputBuffer.Length)
        {
            result.Text.CopyTo(outputBuffer);
            return resultLength;
        }

        return 0;
    }

    #region Core Pipeline

    private static void StemmingWord(ref MorphologyResult result)
    {
        // Delegate to Root Extractor
        ArabicRootExtractor.ExtractRoot(ref result);

        // FIX: Handle "است" + Geminated Root ambiguity (Pattern X vs Pattern VIII)
        // "استمر" -> "مرر" (Pattern X) NOT "سمر" (Pattern VIII)
        // "استلم" -> "سلم" (Pattern VIII) because "لم" is not geminated
        if (!result.IsRootFound && result.Text.Length == 5 && result.Text.StartsWith("است"))
        {
            ReadOnlySpan<char> remainder = result.Text.Slice(3);
            // Check if remainder is a geminated root (e.g., "مر")
            if (ArabicDuplicates.IsDuplicate(remainder))
            {
                // Extract root from the remainder (will expand "مر" -> "مرر")
                Span<char> tempBuffer = stackalloc char[64];
                var tempResult = new MorphologyResult(tempBuffer, remainder, result.IsApplyFuzzyNormalization);
                ArabicRootExtractor.ExtractRoot(ref tempResult);

                if (tempResult.IsRootFound)
                {
                    tempResult.Text.CopyTo(result.RawBuffer);
                    result.UpdateLength(tempResult.Text.Length);
                    result.IsRootFound = true;
                    return;
                }
            }
        }

        if (!result.IsRootFound) CheckPatterns(ref result);

        // Delegate to Affix Stripper
        if (!result.IsRootFound) ArabicAffixStripper.CheckDefiniteArticle(ref result);
        if (!result.IsRootFound && !result.IsStopWord) ArabicAffixStripper.CheckPrefixWaw(ref result);
        // Check prefixes BEFORE suffixes to handle multi-char prefixes like "است" correctly
        if (!result.IsRootFound && !result.IsStopWord) ArabicAffixStripper.CheckPrefixes(ref result);
        if (!result.IsRootFound && !result.IsStopWord) ArabicAffixStripper.CheckSuffixes(ref result);
    }

    /// <summary>
    /// Analyzes a candidate root/stem.
    /// Made public to support recursion from ArabicAffixStripper.
    /// </summary>
    public static void AnalyzeCandidate(ref MorphologyResult parentResult, ReadOnlySpan<char> candidateText)
    {
        Span<char> tempBuffer = stackalloc char[64];
        var tempResult = new MorphologyResult(tempBuffer, candidateText, parentResult.IsApplyFuzzyNormalization);

        if (CheckStopWords(ref tempResult)) goto Finish;

        // Delegate to Root Extractor
        ArabicRootExtractor.ExtractRoot(ref tempResult);

        if (tempResult.IsRootFound) goto Finish;

        if (tempResult.Text.Length > 2) CheckPatterns(ref tempResult);
        if (tempResult.IsRootFound) goto Finish;

        if (!tempResult.IsProcessingSuffixes) ArabicAffixStripper.CheckSuffixes(ref tempResult);
        if (tempResult.IsFinished) goto Finish;

        if (!tempResult.IsFinished) ArabicAffixStripper.CheckPrefixes(ref tempResult);

    Finish:
        if (tempResult.IsFinished)
        {
            tempResult.Text.CopyTo(parentResult.RawBuffer);
            parentResult.UpdateLength(tempResult.Text.Length);
            parentResult.IsRootFound = tempResult.IsRootFound;
            parentResult.IsStopWord = tempResult.IsStopWord;
            parentResult.IsPatternFound = tempResult.IsPatternFound;
            parentResult.IsStrangeWord = tempResult.IsStrangeWord;
        }
    }

    #endregion Core Pipeline

    #region Cleaning Helpers (In-Place Mutation)

    private static void RemoveDiacritics(ref MorphologyResult result)
    {
        int newLength = ArabicDiacritics.RemoveDiacritics(result.Text, result.RawBuffer);
        result.UpdateLength(newLength);
    }

    private static void RemovePunctuation(ref MorphologyResult result)
    {
        int newLength = TextPunctuation.RemovePunctuation(result.Text, result.RawBuffer);
        result.UpdateLength(newLength);
    }

    private static void RemoveNonLetter(ref MorphologyResult result)
    {
        ReadOnlySpan<char> source = result.Text;
        Span<char> destination = result.RawBuffer;

        int i = 0;
        while (i < source.Length && char.IsLetter(source[i])) i++;

        if (i == source.Length) return;

        int pos = i;
        for (; i < source.Length; i++)
        {
            if (char.IsLetter(source[i]))
            {
                destination[pos++] = source[i];
            }
        }

        result.UpdateLength(pos);
    }

    #endregion Cleaning Helpers (In-Place Mutation)

    #region Checking Helpers

    private static bool CheckStrangeWords(ref MorphologyResult result)
    {
        result.IsStrangeWord = ArabicStrange.IsStrangeWord(result.Text);
        return result.IsStrangeWord;
    }

    private static bool CheckStopWords(ref MorphologyResult result)
    {
        result.IsStopWord = ArabicStopWords.IsStopWord(result.Text);
        return result.IsStopWord;
    }

    #endregion Checking Helpers

    #region Pattern Helpers

    private static void CheckPatterns(ref MorphologyResult result)
    {
        if (result.Text.Length > 0)
        {
            char first = result.Text[0];
            if (first == ArabicConstantsChar.AlefHamzaAbove || first == ArabicConstantsChar.AlefHamzaBelow || first == ArabicConstantsChar.AlefMedda)
            {
                result.SetChar(0, ArabicConstantsChar.Alef);
            }
        }

        Span<char> rootBuffer = stackalloc char[3];

        if (ArabicTriPatterns.TryExtractRoot(result.Text, rootBuffer, result.IsApplyFuzzyNormalization))
        {
            rootBuffer.CopyTo(result.RawBuffer);
            result.UpdateLength(3);

            result.IsRootFound = true;
            result.IsPatternFound = true;
        }
    }

    #endregion Pattern Helpers
}
