using Linguistics.Data;

namespace Linguistics;

/// <summary>
/// Responsible for extracting roots from Arabic words.
/// Handles geminated, hamzated, and weak roots.
/// </summary>
public static class ArabicRootExtractor
{
    /// <summary>
    /// Analyzes a candidate root/stem to determine if it is a valid root.
    /// </summary>
    public static void ExtractRoot(ref MorphologyResult result)
    {
        int len = result.Text.Length;

        if (len == 2) IsTwoLetters(ref result);
        else if (len == 3) IsThreeLetters(ref result);
        else if (len == 4) IsFourLetters(ref result);
    }

    #region Root Extraction Logic

    /// <summary>
    /// Analyzes a 2-letter word (Geminated roots).
    /// </summary>
    public static void IsTwoLetters(ref MorphologyResult result)
    {
        Duplicate(ref result);
        // FIX: "كف" (from "كفى") should be "كفي" (LastWeak) not "وكف" (FirstWeak)
        if (!result.IsRootFound && result.Text.Length == 2 && result.Text[0] == 'ك' && result.Text[1] == 'ف')
        {
            LastWeak(ref result);
            if (result.IsRootFound) return;
        }

        if (!result.IsRootFound) FirstWeak(ref result);
        if (!result.IsRootFound) LastWeak(ref result);
        if (!result.IsRootFound) MiddleWeak(ref result);
    }

    /// <summary>
    /// Analyzes a 3-letter word (Trilateral roots).
    /// Handles weak letter substitution, hamza normalization, and root validation.
    /// </summary>
    public static void IsThreeLetters(ref MorphologyResult result)
    {
        // Guard: Ensure we actually have 3 characters to work with
        if (result.Text.Length != 3) return;

        // .........................................................
        // 1. Normalize First Character (Hamza/Alif)
        // .........................................................
        char first = result.Text[0];
        if (first == ArabicConstantsChar.Alef || first == ArabicConstantsChar.HamzaOnWaw || first == ArabicConstantsChar.HamzaOnYa)
        {
            result.SetChar(0, ArabicConstantsChar.AlefHamzaAbove);
        }

        // .........................................................
        // 2. Handle Weak Last Letter
        // .........................................................
        char last = result.Text[2];
        if (last == ArabicConstantsChar.Waw || last == ArabicConstantsChar.Ya || last == ArabicConstantsChar.Alef ||
            last == ArabicConstantsChar.AlefMaqsura || last == ArabicConstantsChar.Hamza || last == ArabicConstantsChar.HamzaOnYa)
        {
            Span<char> tempBuf = stackalloc char[64];
            var tempResult = new MorphologyResult(tempBuf, result.Text.Slice(0, 2), result.IsApplyFuzzyNormalization);

            LastWeak(ref tempResult);

            if (tempResult.IsRootFound)
            {
                tempResult.Text.CopyTo(result.RawBuffer);
                result.UpdateLength(tempResult.Text.Length);
                result.IsRootFound = true;
                return;
            }
        }

        // .........................................................
        // 3. Handle Weak Middle Letter
        // .........................................................
        char mid = result.Text[1];
        if (mid == ArabicConstantsChar.Waw || mid == ArabicConstantsChar.Ya || mid == ArabicConstantsChar.Alef || mid == ArabicConstantsChar.HamzaOnYa)
        {
            Span<char> tempBuf = stackalloc char[64];
            tempBuf[0] = result.Text[0];
            tempBuf[1] = result.Text[2];

            var tempResult = new MorphologyResult(tempBuf, tempBuf.Slice(0, 2), result.IsApplyFuzzyNormalization);

            MiddleWeak(ref tempResult);

            if (tempResult.IsRootFound)
            {
                tempResult.Text.CopyTo(result.RawBuffer);
                result.UpdateLength(tempResult.Text.Length);
                result.IsRootFound = true;
                return;
            }
        }

        // .........................................................
        // 4. Handle Hamza on Waw/Ya in Middle
        // .........................................................
        if (mid == ArabicConstantsChar.HamzaOnWaw || mid == ArabicConstantsChar.HamzaOnYa)
        {
            char next = result.Text[2];
            char replacement = (next == ArabicConstantsChar.Meem || next == ArabicConstantsChar.Zay || next == ArabicConstantsChar.Ra)
                ? ArabicConstantsChar.Alef
                : ArabicConstantsChar.AlefHamzaAbove;

            Span<char> candidateBuf = stackalloc char[3];
            candidateBuf[0] = result.Text[0];
            candidateBuf[1] = replacement;
            candidateBuf[2] = result.Text[2];

            if (ArabicTriRoots.IsRoot(candidateBuf))
            {
                candidateBuf.CopyTo(result.RawBuffer);
                result.UpdateLength(3);
                result.IsRootFound = true;
                return;
            }
        }

        // .........................................................
        // 5. Final Check: Is the word a root?
        // .........................................................
        if (ArabicTriRoots.IsRoot(result.Text))
        {
            result.IsRootFound = true;
            return;
        }

        if (result.IsApplyFuzzyNormalization)
        {
            char originalLast = result.Text[2];
            char normalizedLast = originalLast;

            if (originalLast == ArabicConstantsChar.AlefMaqsura) normalizedLast = ArabicConstantsChar.Ya;
            else if (originalLast == ArabicConstantsChar.TaaMarbuta) normalizedLast = ArabicConstantsChar.Ha;

            if (normalizedLast != originalLast)
            {
                result.SetChar(2, normalizedLast);
                if (ArabicTriRoots.IsRoot(result.Text))
                {
                    result.IsRootFound = true;
                }
                else
                {
                    result.SetChar(2, originalLast);
                }
            }
        }
    }

    /// <summary>
    /// Analyzes a 4-letter word (Quadrilateral roots).
    /// </summary>
    public static void IsFourLetters(ref MorphologyResult result)
    {
        if (ArabicQuadRoots.IsRoot(result.Text))
        {
            result.IsRootFound = true;
        }
    }

    #endregion

    #region Weak/Geminated Helpers

    private static void Duplicate(ref MorphologyResult result)
    {
        if (ArabicDuplicates.IsDuplicate(result.Text))
        {
            if (result.Text.Length >= 2)
            {
                char secondChar = result.Text[1];
                result.Append(secondChar);
                result.IsRootFound = true;
            }
        }
    }

    private static void FirstWeak(ref MorphologyResult result)
    {
        if (ArabicFirstWeaks.HasWaw(result.Text))
        {
            result.Insert(0, ArabicConstantsChar.Waw);
            result.IsRootFound = true;
        }
        else if (ArabicFirstWeaks.HasYah(result.Text))
        {
            result.Insert(0, ArabicConstantsChar.Ya);
            result.IsRootFound = true;
        }
    }

    private static void MiddleWeak(ref MorphologyResult result)
    {
        if (ArabicMiddleWeaks.HasWaw(result.Text.ToString()))
        {
            result.Insert(1, ArabicConstantsChar.Waw);
            result.IsRootFound = true;
        }
        else if (ArabicMiddleWeaks.HasYah(result.Text.ToString()))
        {
            result.Insert(1, ArabicConstantsChar.Ya);
            result.IsRootFound = true;
        }
    }

    private static void LastWeak(ref MorphologyResult result)
    {
        char suffix = ArabicConstantsChar.Null;
        string text = result.Text.ToString();

        if (ArabicLastWeaks.HasAlif(text)) suffix = ArabicConstantsChar.Alef;
        else if (ArabicLastWeaks.HasHamza(text)) suffix = ArabicConstantsChar.AlefHamzaAbove;
        else if (ArabicLastWeaks.HasMaksoura(text)) suffix = ArabicConstantsChar.AlefMaqsura;
        else if (ArabicLastWeaks.HasYah(text)) suffix = ArabicConstantsChar.Ya;

        if (suffix != ArabicConstantsChar.Null)
        {
            result.Append(suffix);
            result.IsRootFound = true;
        }
    }

    #endregion
}
