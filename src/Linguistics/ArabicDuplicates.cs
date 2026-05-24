using System.Runtime.CompilerServices;

using Linguistics.Data;

namespace Linguistics;

/// <summary>
/// High-performance checker for Geminated (Duplicate) roots.
/// Optimization: Packs 2 chars into a uint for O(1) lookup.
/// </summary>
public static class ArabicDuplicates
{
    private static readonly HashSet<uint> encodedRoots;

    static ArabicDuplicates()
    {
        var source = ArabicRootsData.Geminated;
        encodedRoots = new HashSet<uint>(source.Count);

        foreach (string s in source)
        {
            if (s.Length == 2) encodedRoots.Add(Pack(s));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDuplicate(ReadOnlySpan<char> span)
    {
        if (span.Length != 2) return false;
        return encodedRoots.Contains(Pack(span));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Pack(ReadOnlySpan<char> s) => (uint)s[0] | ((uint)s[1] << 16);
}
