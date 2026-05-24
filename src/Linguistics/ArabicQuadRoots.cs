using System.Runtime.CompilerServices;

using Linguistics.Data;

namespace Linguistics;

/// <summary>
/// High-performance, zero-allocation checker for Arabic Quadrilateral Roots (4 letters).
/// Optimization: Packs 4 chars into a ulong for O(1) integer lookup.
/// </summary>
public static class ArabicQuadRoots
{
    // Storage: 64-bit integer representation of roots.
    // 4 chars * 16 bits = 64 bits. Fits exactly in ulong.
    private static readonly HashSet<ulong> encodedRoots;

    static ArabicQuadRoots()
    {
        var source = ArabicRootsData.Quadrilateral;
        encodedRoots = new HashSet<ulong>(source.Count);

        foreach (string root in source)
        {
            if (root.Length == 4)
            {
                encodedRoots.Add(Pack(root));
            }
        }
    }

    /// <summary>
    /// Checks if the span represents a valid quadrilateral root.
    /// Zero-Allocation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRoot(ReadOnlySpan<char> span)
    {
        // 1. Length Guard
        if (span.Length != 4) return false;

        // 2. Pack into ulong (Zero Alloc)
        // We use bitwise OR to combine the 16-bit chars into a 64-bit integer.
        ulong key = (ulong)span[0] |
                   ((ulong)span[1] << 16) |
                   ((ulong)span[2] << 32) |
                   ((ulong)span[3] << 48);

        // 3. Integer Lookup
        return encodedRoots.Contains(key);
    }

    /// <summary>
    /// Helper to pack string into ulong.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Pack(string s)
    {
        return (ulong)s[0] |
              ((ulong)s[1] << 16) |
              ((ulong)s[2] << 32) |
              ((ulong)s[3] << 48);
    }
}
