using Linguistics.Data;

namespace Linguistics;

/// <summary>
/// Represents a compiled pattern for diacritic normalization or removal.
/// Optimized for high-performance scanning with cached length and start character.
/// </summary>
public readonly struct ArabicDiacriticsPattern
{
    public bool IsQuran { get; }
    public string Unicode { get; }
    public string Replacement { get; }
    public bool IsSpecial { get; }

    // Optimization: Cache the length and start char for fast scanning
    public int Length { get; }
    public char StartChar { get; }

    public ArabicDiacriticsPattern(string unicode, string replacement, bool isQuran, bool isSpecial)
    {
        Unicode = unicode;
        Replacement = replacement;
        IsQuran = isQuran;
        IsSpecial = isSpecial;
        Length = unicode.Length;
        StartChar = unicode.Length > 0 ? unicode[0] : ArabicConstantsChar.Null;
    }
}
