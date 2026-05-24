using System.Runtime.CompilerServices;

using Linguistics.Data;

namespace Linguistics;

/// <summary>
/// High-performance, zero-allocation Arabic Pattern Matcher.
/// Optimized using Length Buckets and Pre-calculated Root Indices.
/// </summary>
public static class ArabicTriPatterns
{
    /// <summary>
    /// Represents a compiled morphological pattern.
    /// </summary>
    public readonly struct Pattern
    {
        /// <summary>
        /// The raw pattern string (e.g., "مفاعيل").
        /// </summary>
        public readonly string Template;

        /// <summary>
        /// The index of the first root letter ('ف') in the template.
        /// </summary>
        public readonly int R1;

        /// <summary>
        /// The index of the second root letter ('ع') in the template.
        /// </summary>
        public readonly int R2;

        /// <summary>
        /// The index of the third root letter ('ل') in the template.
        /// </summary>
        public readonly int R3;

        public Pattern(string template)
        {
            Template = template;
            R1 = -1;
            R2 = -1;
            R3 = -1;

            // Pre-calculate root indices to avoid scanning at runtime
            for (int i = 0; i < template.Length; i++)
            {
                char c = template[i];
                if (c == ArabicConstantsChar.Fa) R1 = i;      // Fa
                else if (c == ArabicConstantsChar.Ain) R2 = i; // Ain
                else if (c == ArabicConstantsChar.Lam) R3 = i; // Lam
            }
        }

        /// <summary>
        /// Checks if the word matches this pattern template.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMatch(ReadOnlySpan<char> word)
        {
            // Note: Length check is done by the caller (Bucket selection)
            ReadOnlySpan<char> templateSpan = Template.AsSpan();

            for (int i = 0; i < word.Length; i++)
            {
                char tChar = templateSpan[i];

                // If the template char is a root placeholder, it matches anything.
                // If it is a fixed char (e.g., 'م', 'ا'), it must match the word exactly.
                if (tChar != ArabicConstantsChar.Fa && tChar != ArabicConstantsChar.Ain && tChar != ArabicConstantsChar.Lam)
                {
                    if (tChar != word[i]) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Extracts the root letters directly using pre-calculated indices.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExtractRoot(ReadOnlySpan<char> word, Span<char> destination)
        {
            // Direct access - O(1)
            if (R1 >= 0) destination[0] = word[R1];
            if (R2 >= 0) destination[1] = word[R2];
            if (R3 >= 0) destination[2] = word[R3];
        }
    }

    #region DATA STORAGE

    // ........................................................................
    // DATA STORAGE
    // ........................................................................

    // Buckets: Index = Length of the pattern.
    // _patternsByLength[6] contains all 6-letter patterns.
    private static readonly Pattern[][] patternsByLength;

    private static readonly int maxLength;

    static ArabicTriPatterns()
    {
        var rawPatterns = ArabicPatternsData.MorphologyTemplates;

        if (rawPatterns.Count > 0)
        {
            maxLength = rawPatterns.Max(p => p.Length);
            patternsByLength = new Pattern[maxLength + 1][];

            var grouped = rawPatterns.GroupBy(p => p.Length);

            foreach (var group in grouped)
            {
                patternsByLength[group.Key] = group
                    .Select(p => new Pattern(p))
                    .ToArray();
            }
        }
        else
        {
            patternsByLength = Array.Empty<Pattern[]>();
        }
    }

    #endregion DATA STORAGE

    #region PUBLIC API

    // ........................................................................
    // PUBLIC API
    // ........................................................................

    /// <summary>
    /// Attempts to match the word against known patterns and extract a root.
    /// </summary>
    /// <param name="word">The input word.</param>
    /// <param name="rootBuffer">Buffer to write the extracted root (must be length 3).</param>
    /// <returns>True if a match was found and a valid root extracted.</returns>
    public static bool TryExtractRoot(ReadOnlySpan<char> word, Span<char> rootBuffer, bool applyFuzzyNormalization)
    {
        int len = word.Length;

        // 1. Bounds Check
        if (len == 0 || len > maxLength) return false;

        // 2. Get Bucket
        Pattern[]? bucket = patternsByLength[len];
        if (bucket == null) return false;

        // 3. Iterate Patterns
        foreach (var pattern in bucket)
        {
            if (pattern.IsMatch(word))
            {
                // 4. Extract Root
                pattern.ExtractRoot(word, rootBuffer);

                // 5. Validate Root
                // The extracted letters must form a valid trilateral root.
                // We use the optimized Integer Lookup from ArabicTriRoots.
                if (ArabicTriRoots.IsRoot(rootBuffer))
                {
                    return true;
                }

                if (applyFuzzyNormalization)
                {
                    // Fuzzy Normalization for Extracted Root
                    // The pattern might have extracted 'ى' or 'ة', but the dictionary expects 'ي' or 'ه'.
                    char originalLast = rootBuffer[2];
                    char normalizedLast = originalLast;

                    if (originalLast == ArabicConstantsChar.AlefMaqsura) normalizedLast = ArabicConstantsChar.Ya;      // ى -> ي
                    else if (originalLast == ArabicConstantsChar.TaaMarbuta) normalizedLast = ArabicConstantsChar.Ha; // ة -> ه

                    if (normalizedLast != originalLast)
                    {
                        rootBuffer[2] = normalizedLast;
                        if (ArabicTriRoots.IsRoot(rootBuffer))
                        {
                            return true;
                        }
                        // Revert not strictly necessary here as rootBuffer is scratch,
                        // but good practice if we loop further.
                        rootBuffer[2] = originalLast;
                    }
                }
            }
        }

        return false;
    }

    #endregion PUBLIC API
}
