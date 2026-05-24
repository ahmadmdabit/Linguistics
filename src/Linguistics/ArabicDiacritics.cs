using System.Buffers;
using System.Runtime.CompilerServices;

using Linguistics.Data;

namespace Linguistics;

/// <summary>
/// High-performance, zero-allocation Arabic Diacritic checker.
/// Optimized for .NET 6+ using Bitmasks and String Literal Spans.
/// </summary>
public static class ArabicDiacritics
{
    // Unicode Constants

    private const char BaseChar = ArabicConstantsChar.TanwinFath; // First Diacritic
    private const int BaseCode = ArabicConstantsChar.TanwinFath;    // U+064B
    private const int LastCode = ArabicConstantsChar.VerticalZigzagFatha;    // U+065F
    private const int Range = LastCode - BaseCode + 1; // 21 characters

    #region DATA DEFINITIONS

    // ........................................................................
    // DATA DEFINITIONS
    // ........................................................................
    // In .NET 6, returning a string literal as ReadOnlySpan<char> is zero-allocation.
    // The JIT points the span directly to the string in the assembly data section.

    private const string CommonDiacriticsStr = ArabicConstants.CommonDiacritics;
    private const string QuranicAndMarksStr = ArabicConstants.QuranicMarks;
    private const string RareAndExtendedStr = ArabicConstants.RareMarks;

    #endregion DATA DEFINITIONS

    #region BITMASKS (CPU Register Optimization)

    // ........................................................................
    // BITMASKS (CPU Register Optimization)
    // ........................................................................

    private static readonly uint diacriticMask;
    private static readonly uint commonMask;
    private static readonly uint quranicMask;
    private static readonly uint rareMask;

    // Cached array for tooling/iteration to avoid allocation on property access
    private static readonly char[] allDiacriticsArray;

    static ArabicDiacritics()
    {
        // 1. Build Masks
        commonMask = BuildMask(CommonDiacriticsStr);
        quranicMask = BuildMask(QuranicAndMarksStr);
        rareMask = BuildMask(RareAndExtendedStr);

        // Combine masks using bitwise OR
        diacriticMask = commonMask | quranicMask | rareMask;

        // 2. Pre-allocate the "All" array once
        allDiacriticsArray = (CommonDiacriticsStr + QuranicAndMarksStr + RareAndExtendedStr).ToCharArray();
    }

    /// <summary>
    /// Helper to build bitmask from string.
    /// </summary>
    private static uint BuildMask(ReadOnlySpan<char> span)
    {
        uint mask = 0;
        foreach (char c in span)
        {
            // No bounds check needed here as we control the input strings above
            mask |= 1u << (c - BaseChar);
        }
        return mask;
    }

    #endregion BITMASKS (CPU Register Optimization)

    #region PUBLIC API (Inlined for Performance)

    // ........................................................................
    // PUBLIC API (Inlined for Performance)
    // ........................................................................

    /// <summary>
    /// Checks if the character is any known Arabic diacritic in the range U+064B to U+065F.
    /// Complexity: O(1) - Bitwise operation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDiacritic(char c)
    {
        // 1. Fast Range Check: (c - Base) < Range
        // Casting to uint handles negative results (if c < Base) by wrapping to MaxInt,
        // effectively checking both Lower and Upper bounds in one instruction.
        uint offset = (uint)c - BaseCode;

        if (offset < Range)
        {
            // 2. Bitmask Check
            return ((diacriticMask >> (int)offset) & 1u) != 0;
        }

        // 3. Extended Quranic & Symbol Checks
        // These are the characters used in ArabicDiacriticsPatterns
        return c switch
        {
            ArabicConstantsChar.Tatweel => true, // Tatweel
            ArabicConstantsChar.SmallAlef => true, // Superscript Alef
            ArabicConstantsChar.SuperscriptAlef => true, // Alef Wasla
            // Small High/Low Letters (06D6 - 06ED)
            >= ArabicConstantsChar.SmallHighLigatureSadLamAlef and <= ArabicConstantsChar.SmallLowMeem => true,
            // Extended Vowels (08F0 - 08F3)
            >= ArabicConstantsChar.OpenMarkAbove and <= ArabicConstantsChar.OpenMarkBelow => true,
            // Honorifics/Marks (0610 - 061A)
            >= ArabicConstantsChar.SmallHighTah and <= ArabicConstantsChar.SmallHighSeen => true,
            // Symbols
            ArabicConstantsChar.RumiDigitOne or ArabicConstantsChar.RumiDigitTwo => true,
            _ => false
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCommonDiacritic(char c)
    {
        uint offset = (uint)c - BaseCode;
        return offset < Range && ((commonMask >> (int)offset) & 1u) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsQuranicOrMark(char c)
    {
        uint offset = (uint)c - BaseCode;
        return offset < Range && ((quranicMask >> (int)offset) & 1u) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRareOrExtended(char c)
    {
        uint offset = (uint)c - BaseCode;
        return offset < Range && ((rareMask >> (int)offset) & 1u) != 0;
    }

    #endregion PUBLIC API (Inlined for Performance)

    #region TOOLING / ITERATION

    // ........................................................................
    // TOOLING / ITERATION
    // ........................................................................

    /// <summary>
    /// Returns a ReadOnlySpan of all diacritics.
    /// Zero-allocation (points to cached array).
    /// </summary>
    public static ReadOnlySpan<char> AllDiacritics => allDiacriticsArray;

    /// <summary>
    /// Returns a HashSet for scenarios requiring set operations.
    /// Note: This allocates memory. Prefer IsDiacritic() for checks.
    /// </summary>
    public static HashSet<char> AsHashSet()
    {
        // Use the pre-allocated array to initialize the set faster
        return new HashSet<char>(allDiacriticsArray);
    }

    // Expose categories as Spans if needed for specific iteration
    public static ReadOnlySpan<char> CommonDiacritics => CommonDiacriticsStr;

    public static ReadOnlySpan<char> QuranicAndMarks => QuranicAndMarksStr;
    public static ReadOnlySpan<char> RareAndExtended => RareAndExtendedStr;

    #endregion TOOLING / ITERATION

    #region TEXT PROCESSING METHODS

    #region REPLACEMENT / NORMALIZATION API

    /// <summary>
    /// Normalizes the text by applying the provided replacement patterns.
    /// Handles buffer management internally using ArrayPool to ensure zero-allocation on the heap.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <param name="patterns">The list of patterns (Must be sorted by Length Descending for greedy matching).</param>
    /// <returns>The normalized string.</returns>
    public static string Normalize(string text, ArabicDiacriticsPattern[] patterns)
    {
        if (string.IsNullOrEmpty(text)) return text;
        if (patterns == null || patterns.Length == 0) return text;

        ReadOnlySpan<char> source = text.AsSpan();

        // 1. Calculate worst-case size.
        // Replacements usually shrink text, but could expand (rarely).
        // We assume input length is sufficient, but to be safe we can check max expansion.
        // For Arabic diacritics, replacement is almost always <= source.
        int capacity = source.Length;

        char[] rentedBuffer = ArrayPool<char>.Shared.Rent(capacity);

        try
        {
            Span<char> destination = rentedBuffer.AsSpan();

            // 2. Perform the replacement
            int written = Replace(source, destination, patterns);

            // 3. Materialize string
            // Optimization: If nothing changed and lengths match, return original (if exact match logic allows)
            // But usually, we just return the new string.
            return destination.Slice(0, written).ToString();
        }
        finally
        {
            ArrayPool<char>.Shared.Return(rentedBuffer);
        }
    }

    /// <summary>
    /// Core Replacement Engine.
    /// Applies patterns greedily (longest match first) to the source span.
    /// </summary>
    /// <param name="source">Input span.</param>
    /// <param name="destination">Output buffer (must be large enough).</param>
    /// <param name="patterns">Patterns sorted by Length Descending.</param>
    /// <returns>Number of characters written to destination.</returns>
    public static int Replace(ReadOnlySpan<char> source, Span<char> destination, ArabicDiacriticsPattern[] patterns)
    {
        int readIdx = 0;
        int writeIdx = 0;
        int srcLen = source.Length;

        while (readIdx < srcLen)
        {
            bool matched = false;
            char currentChar = source[readIdx];

            // Optimization: Fast Scan
            // Only iterate patterns if the current char matches the start of ANY pattern.
            // We can check this via a helper or just iterate.
            // Given ~50 patterns, iterating all is costly.
            // Let's iterate, but the patterns should be filtered by StartChar ideally.
            // However, for simplicity and robustness without complex lookups:

            for (int i = 0; i < patterns.Length; i++)
            {
                ref readonly var pattern = ref patterns[i];

                // 1. Fast Fail: First char check
                if (pattern.StartChar != currentChar) continue;

                // 2. Bounds Check
                if (readIdx + pattern.Length > srcLen) continue;

                // 3. Full Match Check
                // We use Slice + SequenceEqual (highly optimized in .NET)
                if (source.Slice(readIdx, pattern.Length).SequenceEqual(pattern.Unicode.AsSpan()))
                {
                    // MATCH FOUND!

                    // A. Copy Replacement
                    if (!string.IsNullOrEmpty(pattern.Replacement))
                    {
                        ReadOnlySpan<char> repl = pattern.Replacement.AsSpan();
                        repl.CopyTo(destination.Slice(writeIdx));
                        writeIdx += repl.Length;
                    }

                    // B. Advance Reader
                    readIdx += pattern.Length;
                    matched = true;
                    break; // Break pattern loop, continue main loop
                }
            }

            if (!matched)
            {
                // No pattern matched, copy original character
                destination[writeIdx++] = currentChar;
                readIdx++;
            }
        }

        return writeIdx;
    }

    #endregion REPLACEMENT / NORMALIZATION API

    // ........................................................................
    // TEXT PROCESSING METHODS
    // ........................................................................

    /// <summary>
    /// Checks if the input text contains any Arabic diacritics.
    /// Optimized for speed using Span and Inlining.
    /// </summary>
    /// <param name="text">The text to check (String or Span).</param>
    /// <returns>True if at least one diacritic is found.</returns>
    public static bool HasDiacritics(ReadOnlySpan<char> text)
    {
        // Iterate using Span to avoid bounds checking overhead in modern .NET
        foreach (char c in text)
        {
            if (IsDiacritic(c))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Removes all Arabic diacritics from the string.
    /// Implements "Zero-Allocation" pattern: returns original string if no changes needed.
    /// Uses Stack allocation for short strings to reduce GC pressure.
    /// </summary>
    /// <param name="text">The input string.</param>
    /// <returns>A new string with diacritics removed, or the original string if none were found.</returns>
    public static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        ReadOnlySpan<char> inputSpan = text.AsSpan();
        int firstDiacriticIndex = -1;

        // 1. Fast Scan: Find the first diacritic.
        // This avoids allocating a buffer if the string is already clean.
        for (int i = 0; i < inputSpan.Length; i++)
        {
            if (IsDiacritic(inputSpan[i]))
            {
                firstDiacriticIndex = i;
                break;
            }
        }

        // Optimization: If no diacritics found, return the original instance.
        // This is a massive win for performance (0 allocations).
        if (firstDiacriticIndex == -1)
        {
            return text;
        }

        // 2. Allocation Strategy
        // We need to construct a new string.
        // - Use Stack (stackalloc) for small strings (fastest, no GC).
        // - Use ArrayPool for large strings (prevents heap fragmentation).
        int length = inputSpan.Length;
        char[]? rentedArray = null;

        // 1024 chars = 2KB, safe for stack.
        Span<char> buffer = length <= 1024
            ? stackalloc char[length]
            : (rentedArray = ArrayPool<char>.Shared.Rent(length));

        try
        {
            // 3. Copy the clean prefix (everything before the first diacritic)
            // This uses efficient memory block copy (memcpy).
            inputSpan.Slice(0, firstDiacriticIndex).CopyTo(buffer);

            int pos = firstDiacriticIndex;

            // 4. Process the remainder
            for (int i = firstDiacriticIndex + 1; i < length; i++)
            {
                char c = inputSpan[i];
                if (!IsDiacritic(c))
                {
                    buffer[pos++] = c;
                }
            }

            // 5. Create the final string
            // We slice the buffer to 'pos' because the result is shorter than the input.
            return buffer.Slice(0, pos).ToString();
        }
        finally
        {
            // Return the array to the pool if we rented one
            if (rentedArray != null)
            {
                ArrayPool<char>.Shared.Return(rentedArray);
            }
        }
    }

    /// <summary>
    /// Removes diacritics from the source span and writes the result to the destination span.
    /// Supports in-place modification (source and destination can be the same buffer).
    /// </summary>
    /// <param name="source">The input text.</param>
    /// <param name="destination">The buffer to write to (must be at least as large as source).</param>
    /// <returns>The number of characters written to the destination.</returns>
    public static int RemoveDiacritics(ReadOnlySpan<char> source, Span<char> destination)
    {
        if (destination.Length < source.Length)
            throw new ArgumentException("Destination buffer is too small.");

        // 1. Fast Scan: Find the first diacritic
        int i = 0;
        while (i < source.Length && !IsDiacritic(source[i]))
        {
            i++;
        }

        // Optimization: If no diacritics found
        if (i == source.Length)
        {
            // If buffers are different, we must copy the whole thing.
            // If they are the same (in-place), we do nothing.
            if (source != destination)
            {
                source.CopyTo(destination);
            }
            return source.Length;
        }

        // 2. Copy the clean prefix (if buffers are different)
        // If in-place, the data is already there, so we skip copying.
        if (i > 0 && source != destination)
        {
            source.Slice(0, i).CopyTo(destination);
        }

        // 3. Filter the remainder
        // We use 'pos' as the write cursor.
        int pos = i;
        for (; i < source.Length; i++)
        {
            char c = source[i];
            if (!IsDiacritic(c))
            {
                destination[pos++] = c;
            }
        }

        return pos;
    }

    #endregion TEXT PROCESSING METHODS
}
