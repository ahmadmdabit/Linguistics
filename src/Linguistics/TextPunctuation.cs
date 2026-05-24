using System.Buffers;
using System.Runtime.CompilerServices;

namespace Linguistics;

/// <summary>
/// High-performance, zero-allocation Punctuation checker and remover.
/// Optimized for .NET 6+ using Lookup Tables and Stack Allocation.
/// </summary>
public static class TextPunctuation
{
    #region DATA DEFINITIONS

    // ........................................................................
    // DATA DEFINITIONS
    // ........................................................................

    // Fast lookup table for ASCII characters (0-127).
    // 128 bytes is negligible memory and fits in L1 Cache.
    private static readonly bool[] asciiLookup = new bool[128];

    static TextPunctuation()
    {
        // 1. Populate ASCII Punctuation from the user's original list
        // We map these to the lookup table for O(1) access.
        ReadOnlySpan<char> asciiSymbols = new char[]
        {
            '.', ',', ':', '"', '\'', '>', '<', '|', '\\', '?', '!',
            '@', '#', '$', '%', '^', '&', '*', ')', '(', '_', '-',
            '+', '=', ';', '~', '/', '[', ']', '{', '}', '`'
        };

        foreach (char c in asciiSymbols)
        {
            if (c < 128) asciiLookup[c] = true;
        }
    }

    #endregion DATA DEFINITIONS

    #region PUBLIC API (Inlined for Performance)

    // ........................................................................
    // PUBLIC API (Inlined for Performance)
    // ........................................................................

    /// <summary>
    /// Checks if the character is considered punctuation based on the specific project rules.
    /// Includes standard ASCII symbols and specific Arabic/Unicode punctuation.
    /// Complexity: O(1).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPunctuation(char c)
    {
        // Path 1: Fast ASCII Lookup (0-127)
        // Covers: . , : " ' > < | \ ? ! @ # $ % ^ & * ) ( _ - + = ; ~ / [ ] { } `
        if (c < 128)
        {
            return asciiLookup[c];
        }

        // Path 2: Unicode Specifics
        // This compiles to an optimized jump table (binary search or direct jump).
        return c switch
        {
            // --- Arabic Specific Punctuation ---
            '،' => true, // Arabic Comma (U+060C)
            '؛' => true, // Arabic Semicolon (U+061B)
            '؟' => true, // Arabic Question Mark (U+061F)

            // --- Extended/Latin Punctuation ---
            '¡' => true, // Inverted Exclamation (U+00A1)
            '¿' => true, // Inverted Question (U+00BF)
            '÷' => true, // Division Sign (U+00F7)
            '×' => true, // Multiplication Sign (U+00D7)
            'º' => true, // Masculine Ordinal Indicator (U+00BA)
            'ø' => true, // Latin Small Letter O with Stroke (Legacy artifact)

            // Default
            _ => false
        };
    }

    /// <summary>
    /// Checks if the input text contains any punctuation.
    /// Optimized for speed using Span and Inlining.
    /// </summary>
    public static bool HasPunctuation(ReadOnlySpan<char> text)
    {
        foreach (char c in text)
        {
            if (IsPunctuation(c))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Removes all defined punctuation from the string.
    /// Implements "Zero-Allocation" pattern: returns original string if no changes needed.
    /// Uses Stack allocation for short strings to reduce GC pressure.
    /// </summary>
    public static string RemovePunctuation(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        ReadOnlySpan<char> inputSpan = text.AsSpan();
        int firstPuncIndex = -1;

        // 1. Fast Scan: Find the first punctuation mark.
        for (int i = 0; i < inputSpan.Length; i++)
        {
            if (IsPunctuation(inputSpan[i]))
            {
                firstPuncIndex = i;
                break;
            }
        }

        // Optimization: If clean, return original instance (0 Allocations).
        if (firstPuncIndex == -1)
        {
            return text;
        }

        // 2. Allocation Strategy (Stack vs Pool)
        int length = inputSpan.Length;
        char[]? rentedArray = null;

        // Use stack for strings <= 1024 chars (2KB), otherwise rent from pool.
        Span<char> buffer = length <= 1024
            ? stackalloc char[length]
            : (rentedArray = ArrayPool<char>.Shared.Rent(length));

        try
        {
            // 3. Copy the clean prefix
            inputSpan.Slice(0, firstPuncIndex).CopyTo(buffer);

            int pos = firstPuncIndex;

            // 4. Process the remainder
            for (int i = firstPuncIndex + 1; i < length; i++)
            {
                char c = inputSpan[i];
                if (!IsPunctuation(c))
                {
                    buffer[pos++] = c;
                }
            }

            // 5. Create the final string
            return buffer.Slice(0, pos).ToString();
        }
        finally
        {
            if (rentedArray != null)
            {
                ArrayPool<char>.Shared.Return(rentedArray);
            }
        }
    }

    /// <summary>
    /// Removes punctuation from the source span and writes the result to the destination span.
    /// Supports in-place modification.
    /// </summary>
    /// <returns>The number of characters written.</returns>
    public static int RemovePunctuation(ReadOnlySpan<char> source, Span<char> destination)
    {
        if (destination.Length < source.Length)
            throw new ArgumentException("Destination buffer is too small.");

        int i = 0;
        while (i < source.Length && !IsPunctuation(source[i]))
        {
            i++;
        }

        if (i == source.Length)
        {
            if (source != destination) source.CopyTo(destination);
            return source.Length;
        }

        if (i > 0 && source != destination)
        {
            source.Slice(0, i).CopyTo(destination);
        }

        int pos = i;
        for (; i < source.Length; i++)
        {
            char c = source[i];
            if (!IsPunctuation(c))
            {
                destination[pos++] = c;
            }
        }

        return pos;
    }

    #endregion PUBLIC API (Inlined for Performance)
}
