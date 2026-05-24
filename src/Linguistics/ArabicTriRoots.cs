using System.Runtime.CompilerServices;

using Linguistics.Data;

namespace Linguistics;

/// <summary>
/// High-performance, zero-allocation checker for Arabic Trilateral Roots (3 letters).
/// Optimization: Packs 3 chars into a ulong for O(1) integer lookup.
/// </summary>
public static class ArabicTriRoots
{
    // Storage: 64-bit integer representation of roots.
    // 3 chars * 16 bits = 48 bits. Fits easily in ulong.
    private static readonly HashSet<ulong> encodedRoots;

    static ArabicTriRoots()
    {
        // Initialize with capacity to avoid resizing overhead
        var source = ArabicRootsData.Trilateral;
        encodedRoots = new HashSet<ulong>(source.Count);

        foreach (string root in source)
        {
            if (root.Length == 3)
            {
                encodedRoots.Add(Pack(root));
            }
        }
    }

    /// <summary>
    /// Checks if the span represents a valid trilateral root.
    /// Zero-Allocation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRoot(ReadOnlySpan<char> span)
    {
        // 1. Length Guard
        if (span.Length != 3) return false;

        // 2. Pack into ulong (Zero Alloc)
        ulong key = (ulong)span[0] |
                   ((ulong)span[1] << 16) |
                   ((ulong)span[2] << 32);

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
              ((ulong)s[2] << 32);
    }
}
