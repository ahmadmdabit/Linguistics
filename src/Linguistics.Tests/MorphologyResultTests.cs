namespace Linguistics.Tests;

/// <summary>
/// Comprehensive test suite for MorphologyResult ref struct.
/// Tests all 9 methods with buffer management, edge cases, and boundary conditions.
/// Focus: Span-based operations, stack allocation, zero-allocation patterns.
/// </summary>
[TestFixture]
public class MorphologyResultTests
{
    #region Constructor Tests

    [Test]
    public void Constructor_ValidInput_ShouldInitialize()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        ReadOnlySpan<char> initialText = "كتاب";

        // Act
        var result = new MorphologyResult(buffer, initialText, isApplyFuzzyNormalization: false);

        // Assert
        result.Text.ToString().Should().Be("كتاب");
        result.IsApplyFuzzyNormalization.Should().BeFalse();
        result.IsFinished.Should().BeFalse();
    }

    [Test]
    public void Constructor_EmptyText_ShouldWork()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        ReadOnlySpan<char> initialText = "";

        // Act
        var result = new MorphologyResult(buffer, initialText, isApplyFuzzyNormalization: true);

        // Assert
        result.Text.Length.Should().Be(0);
        result.IsApplyFuzzyNormalization.Should().BeTrue();
    }

    [Test]
    public void Constructor_TextTooLarge_ShouldThrow()
    {
        // Arrange
        Span<char> buffer = stackalloc char[5];
        ReadOnlySpan<char> initialText = "This text is too long";

        // Act & Assert
        bool threw = false;
        try
        {
            var _ = new MorphologyResult(buffer, initialText, false);
        }
        catch (ArgumentException ex)
        {
            threw = true;
            ex.Message.Should().Contain("exceeds buffer size");
        }
        threw.Should().BeTrue("constructor should throw ArgumentException for text exceeding buffer size");
    }

    [Test]
    public void Constructor_FlagsInitialized_ShouldBeFalse()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        ReadOnlySpan<char> initialText = "test";

        // Act
        var result = new MorphologyResult(buffer, initialText, false);

        // Assert
        result.IsRootFound.Should().BeFalse();
        result.IsStopWord.Should().BeFalse();
        result.IsStrangeWord.Should().BeFalse();
        result.IsPatternFound.Should().BeFalse();
        result.IsProcessingSuffixes.Should().BeFalse();
    }

    #endregion

    #region SetText Tests

    [Test]
    public void SetText_NewText_ShouldReplace()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "original", false);

        // Act
        result.SetText("replaced");

        // Assert
        result.Text.ToString().Should().Be("replaced");
    }

    [Test]
    public void SetText_EmptyText_ShouldClear()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "original", false);

        // Act
        result.SetText("");

        // Assert
        result.Text.Length.Should().Be(0);
    }

    [Test]
    public void SetText_TextTooLarge_ShouldThrow()
    {
        // Arrange
        Span<char> buffer = stackalloc char[5];
        var result = new MorphologyResult(buffer, "abc", false);

        // Act & Assert
        bool threw = false;
        try
        {
            result.SetText("This is way too long");
        }
        catch (InvalidOperationException ex)
        {
            threw = true;
            ex.Message.Should().Contain("overflow");
        }
        threw.Should().BeTrue("SetText should throw InvalidOperationException for overflow");
    }

    #endregion

    #region TrimStart Tests

    [Test]
    public void TrimStart_RemoveChars_ShouldShiftLeft()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "الكتاب", false); // "the book"

        // Act
        result.TrimStart(2); // Remove "ال" (the)

        // Assert
        result.Text.ToString().Should().Be("كتاب"); // "book"
    }

    [Test]
    public void TrimStart_ZeroCount_ShouldDoNothing()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "test", false);

        // Act
        result.TrimStart(0);

        // Assert
        result.Text.ToString().Should().Be("test");
    }

    [Test]
    public void TrimStart_NegativeCount_ShouldDoNothing()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "test", false);

        // Act
        result.TrimStart(-5);

        // Assert
        result.Text.ToString().Should().Be("test");
    }

    [Test]
    public void TrimStart_CountExceedsLength_ShouldClearAll()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "test", false);

        // Act
        result.TrimStart(10);

        // Assert
        result.Text.Length.Should().Be(0);
    }

    [Test]
    public void TrimStart_RemoveAll_ShouldClear()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "test", false);

        // Act
        result.TrimStart(4);

        // Assert
        result.Text.Length.Should().Be(0);
    }

    #endregion

    #region TrimEnd Tests

    [Test]
    public void TrimEnd_RemoveChars_ShouldShortenLength()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "كتابي", false); // "my book"

        // Act
        result.TrimEnd(1); // Remove "ي" (my)

        // Assert
        result.Text.ToString().Should().Be("كتاب"); // "book"
    }

    [Test]
    public void TrimEnd_ZeroCount_ShouldDoNothing()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "test", false);

        // Act
        result.TrimEnd(0);

        // Assert
        result.Text.ToString().Should().Be("test");
    }

    [Test]
    public void TrimEnd_NegativeCount_ShouldDoNothing()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "test", false);

        // Act
        result.TrimEnd(-3);

        // Assert
        result.Text.ToString().Should().Be("test");
    }

    [Test]
    public void TrimEnd_CountExceedsLength_ShouldClearAll()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "test", false);

        // Act
        result.TrimEnd(10);

        // Assert
        result.Text.Length.Should().Be(0);
    }

    #endregion

    #region Append Tests

    [Test]
    public void Append_Char_ShouldAddCharacter()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "كتا", false);

        // Act
        result.Append('ب');

        // Assert
        result.Text.ToString().Should().Be("كتاب");
    }

    [Test]
    public void Append_Span_ShouldAddText()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "كت", false);

        // Act
        result.Append("اب");

        // Assert
        result.Text.ToString().Should().Be("كتاب");
    }

    [Test]
    public void Append_Char_BufferFull_ShouldFailSilently()
    {
        // Arrange - Fill buffer to capacity
        Span<char> buffer = stackalloc char[5];
        var result = new MorphologyResult(buffer, "12345", false);

        // Act
        result.Append('X'); // Should fail silently

        // Assert
        result.Text.Length.Should().Be(5);
        result.Text.ToString().Should().Be("12345");
    }

    [Test]
    public void Append_Span_BufferFull_ShouldFailSilently()
    {
        // Arrange
        Span<char> buffer = stackalloc char[10];
        var result = new MorphologyResult(buffer, "1234567890", false);

        // Act
        result.Append("XYZ"); // Should fail silently

        // Assert
        result.Text.Length.Should().Be(10);
    }

    #endregion

    #region Insert Tests

    [Test]
    public void Insert_AtBeginning_ShouldInsert()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "تاب", false);

        // Act
        result.Insert(0, 'ك');

        // Assert
        result.Text.ToString().Should().Be("كتاب");
    }

    [Test]
    public void Insert_AtMiddle_ShouldInsert()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "كاب", false);

        // Act
        result.Insert(1, 'ت');

        // Assert
        result.Text.ToString().Should().Be("كتاب");
    }

    [Test]
    public void Insert_AtEnd_ShouldAppend()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "كتا", false);

        // Act
        result.Insert(3, 'ب');

        // Assert
        result.Text.ToString().Should().Be("كتاب");
    }

    [Test]
    public void Insert_InvalidIndex_ShouldFailSilently()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "test", false);

        // Act
        result.Insert(-1, 'X'); // Invalid index

        // Assert
        result.Text.ToString().Should().Be("test");
    }

    [Test]
    public void Insert_IndexOutOfRange_ShouldFailSilently()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "test", false);

        // Act
        result.Insert(10, 'X'); // Index > length

        // Assert
        result.Text.ToString().Should().Be("test");
    }

    #endregion

    #region SetChar Tests

    [Test]
    public void SetChar_ValidIndex_ShouldModify()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "كتاب", false);

        // Act
        result.SetChar(2, 'و'); // Change third letter

        // Assert
        result.Text.ToString().Should().Be("كتوب");
    }

    [Test]
    public void SetChar_FirstChar_ShouldModify()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "كتاب", false);

        // Act
        result.SetChar(0, 'ل');

        // Assert
        result.Text.ToString().Should().Be("لتاب");
    }

    [Test]
    public void SetChar_LastChar_ShouldModify()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "كتاب", false);

        // Act
        result.SetChar(3, 'ة');

        // Assert
        result.Text.ToString().Should().Be("كتاة");
    }

    [Test]
    public void SetChar_InvalidIndex_ShouldDoNothing()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "test", false);

        // Act
        result.SetChar(-1, 'X');
        result.SetChar(10, 'Y');

        // Assert
        result.Text.ToString().Should().Be("test");
    }

    #endregion

    #region UpdateLength Tests

    [Test]
    public void UpdateLength_ValidLength_ShouldUpdate()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "كتاب", false);

        // Act
        result.UpdateLength(2);

        // Assert
        result.Text.Length.Should().Be(2);
        result.Text.ToString().Should().Be("كت");
    }

    [Test]
    public void UpdateLength_Zero_ShouldClear()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "test", false);

        // Act
        result.UpdateLength(0);

        // Assert
        result.Text.Length.Should().Be(0);
    }

    [Test]
    public void UpdateLength_NegativeLength_ShouldThrow()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "test", false);

        // Act & Assert
        bool threw = false;
        try
        {
            result.UpdateLength(-1);
        }
        catch (ArgumentOutOfRangeException)
        {
            threw = true;
        }
        threw.Should().BeTrue("UpdateLength should throw ArgumentOutOfRangeException for negative length");
    }

    [Test]
    public void UpdateLength_ExceedsBufferSize_ShouldThrow()
    {
        // Arrange
        Span<char> buffer = stackalloc char[10];
        var result = new MorphologyResult(buffer, "test", false);

        // Act & Assert
        bool threw = false;
        try
        {
            result.UpdateLength(15);
        }
        catch (ArgumentOutOfRangeException)
        {
            threw = true;
        }
        threw.Should().BeTrue("UpdateLength should throw ArgumentOutOfRangeException for length exceeding buffer size");
    }

    #endregion

    #region IsFinished Property Tests

    [Test]
    public void IsFinished_NoFlagsSet_ShouldReturnFalse()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "test", false);

        // Act & Assert
        result.IsFinished.Should().BeFalse();
    }

    [Test]
    public void IsFinished_RootFound_ShouldReturnTrue()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "test", false);
        result.IsRootFound = true;

        // Act & Assert
        result.IsFinished.Should().BeTrue();
    }

    [Test]
    public void IsFinished_StopWord_ShouldReturnTrue()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "test", false);
        result.IsStopWord = true;

        // Act & Assert
        result.IsFinished.Should().BeTrue();
    }

    [Test]
    public void IsFinished_StrangeWord_ShouldReturnTrue()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "test", false);
        result.IsStrangeWord = true;

        // Act & Assert
        result.IsFinished.Should().BeTrue();
    }

    #endregion

    #region RawBuffer Tests

    [Test]
    public void RawBuffer_ShouldAllowDirectAccess()
    {
        // Arrange
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "test", false);

        // Act - Direct buffer access
        Span<char> rawBuffer = result.RawBuffer;
        "newvalue".AsSpan().CopyTo(rawBuffer);
        result.UpdateLength(8);

        // Assert
        result.Text.ToString().Should().Be("newvalue");
    }

    #endregion
}
