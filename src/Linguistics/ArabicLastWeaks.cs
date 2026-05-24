using System.Runtime.CompilerServices;

using Linguistics.Data;

namespace Linguistics;

/// <summary>
/// High-performance checker for roots ending with a weak letter (Waw/Yah).
/// Optimization: Packs 2 chars into a uint for O(1) lookup.
/// </summary>
public static class ArabicLastWeaks
{
    private static readonly HashSet<uint> alifSet;
    private static readonly HashSet<uint> hamzaSet;
    private static readonly HashSet<uint> maksouraSet;
    private static readonly HashSet<uint> yahSet;

    static ArabicLastWeaks()
    {
        alifSet = EncodeSet(ArabicWeaksData.LastAlif);
        hamzaSet = EncodeSet(ArabicWeaksData.LastHamza);
        maksouraSet = EncodeSet(ArabicWeaksData.LastMaksoura);
        yahSet = EncodeSet(ArabicWeaksData.LastYah);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasAlif(ReadOnlySpan<char> span) => Check(span, alifSet);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasHamza(ReadOnlySpan<char> span) => Check(span, hamzaSet);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasMaksoura(ReadOnlySpan<char> span) => Check(span, maksouraSet);

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
