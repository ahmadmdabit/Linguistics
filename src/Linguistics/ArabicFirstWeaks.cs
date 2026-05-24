using System.Runtime.CompilerServices;

using Linguistics.Data;

namespace Linguistics;

/// <summary>
/// High-performance checker for roots starting with a weak letter (Waw/Yah).
/// Optimization: Packs 2 chars into a uint for O(1) lookup.
/// </summary>
public static class ArabicFirstWeaks
{
    private static readonly HashSet<uint> wawSet;
    private static readonly HashSet<uint> yahSet;

    static ArabicFirstWeaks()
    {
        wawSet = EncodeSet(ArabicWeaksData.FirstWaw);
        yahSet = EncodeSet(ArabicWeaksData.FirstYah);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasWaw(ReadOnlySpan<char> span) => Check(span, wawSet);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasYah(ReadOnlySpan<char> span) => Check(span, yahSet);

    // ...... Helpers ......

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Check(ReadOnlySpan<char> span, HashSet<uint> set)
    {
        if (span.Length != 2) return false;
        return set.Contains((uint)span[0] | ((uint)span[1] << 16));
    }

    private static HashSet<uint> EncodeSet(HashSet<string> source)
    {
        var set = new HashSet<uint>(source.Count);
        foreach (string s in source)
        {
            if (s.Length == 2) set.Add((uint)s[0] | ((uint)s[1] << 16));
        }
        return set;
    }
}
