/// ...................................................................................................
/// NOTE:
/// Do not clean-up this file using Ctrl+M, Ctrl+Space in Visual Studio. It must remain a 'ref struct'.
/// ...................................................................................................
namespace Linguistics;

/// <summary>
/// A high-performance, zero-allocation mutable string buffer.
/// Must be a 'ref struct' to hold Span<char>.
/// </summary>
public ref struct MorphologyResult
{
    // The underlying storage (provided by the caller via stackalloc)
    private readonly Span<char> buffer;

    // The number of characters currently in use
    private int length;

    public bool IsApplyFuzzyNormalization { get; }

    // --- State Flags ---
    public bool IsRootFound;
    public bool IsStopWord;
    public bool IsStrangeWord;
    public bool IsPatternFound;
    public bool IsProcessingSuffixes;

    public bool IsFinished => IsRootFound || IsStopWord || IsStrangeWord;

    /// <summary>
    /// Initializes the result with a buffer and initial text.
    /// </summary>
    /// <param name="buffer">The fixed-size buffer (stackalloc).</param>
    /// <param name="initialText">The initial text to copy into the buffer.</param>
    public MorphologyResult(Span<char> buffer, ReadOnlySpan<char> initialText, bool isApplyFuzzyNormalization)
    {
        if (initialText.Length > buffer.Length)
            throw new ArgumentException("Initial text exceeds buffer size.");

        IsApplyFuzzyNormalization = isApplyFuzzyNormalization;

        this.buffer = buffer;
        initialText.CopyTo(this.buffer);
        length = initialText.Length;

        // Reset flags
        IsRootFound = false;
        IsStopWord = false;
        IsStrangeWord = false;
        IsPatternFound = false;
        IsProcessingSuffixes = false;
    }

    /// <summary>
    /// Gets the current text as a ReadOnlySpan.
    /// </summary>
    public ReadOnlySpan<char> Text => buffer.Slice(0, length);

    // ...... FIX FOR COMPILER ERROR ......

    /// <summary>
    /// Exposes the full capacity buffer. 
    /// Allows external code to perform safe Span.CopyTo() operations.
    /// </summary>
    public Span<char> RawBuffer => buffer;

    /// <summary>
    /// Updates the length after writing to RawBuffer.
    /// </summary>
    public void UpdateLength(int newLength)
    {
        if (newLength < 0 || newLength > buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(newLength));
        length = newLength;
    }

    // ...... Standard Methods ......

    /// <summary>
    /// Replaces the current text with new text.
    /// </summary>
    public void SetText(ReadOnlySpan<char> newText)
    {
        if (newText.Length > buffer.Length)
            throw new InvalidOperationException("Buffer overflow.");

        newText.CopyTo(buffer);
        length = newText.Length;
    }

    /// <summary>
    /// Removes characters from the start of the string (Shift Left).
    /// </summary>
    public void TrimStart(int count)
    {
        if (count <= 0) return;
        if (count >= length)
        {
            length = 0;
            return;
        }

        // Move memory: src=count, dst=0, len=_length-count
        buffer.Slice(count, length - count).CopyTo(buffer);
        length -= count;
    }

    /// <summary>
    /// Removes characters from the end of the string.
    /// </summary>
    public void TrimEnd(int count)
    {
        if (count <= 0) return;
        if (count >= length) length = 0;
        else length -= count;
    }

    /// <summary>
    /// Appends a character to the end.
    /// </summary>
    public void Append(char c)
    {
        if (length >= buffer.Length) return; // Fail silently or throw based on preference
        buffer[length++] = c;
    }

    /// <summary>
    /// Appends a string to the end.
    /// </summary>
    public void Append(ReadOnlySpan<char> text)
    {
        if (length + text.Length > buffer.Length) return;
        text.CopyTo(buffer.Slice(length));
        length += text.Length;
    }

    /// <summary>
    /// Inserts a character at the specified index.
    /// </summary>
    public void Insert(int index, char c)
    {
        if (length >= buffer.Length) return;
        if (index < 0 || index > length) return;

        // Shift right to make space
        buffer.Slice(index, length - index).CopyTo(buffer.Slice(index + 1));
        buffer[index] = c;
        length++;
    }

    /// <summary>
    /// Modifies a character at a specific index.
    /// </summary>
    public void SetChar(int index, char c)
    {
        if (index >= 0 && index < length)
        {
            buffer[index] = c;
        }
    }
}
