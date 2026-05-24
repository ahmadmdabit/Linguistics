namespace Linguistics.Tests;

/// <summary>
/// Comprehensive test suite for TextPunctuation class.
/// Tests all 4 public methods with ASCII, Arabic, and Unicode punctuation.
/// Focus: Lookup table optimization, zero-allocation patterns.
/// </summary>
[TestFixture]
public class TextPunctuationTests
{
    #region IsPunctuation Tests

    [Test]
    public void IsPunctuation_ASCIIPunctuation_ShouldReturnTrue()
    {
        // Arrange - Standard ASCII punctuation
        char[] asciiPunctuation = {
            '.', ',', ':', ';', '!', '?',
            '"', '\'', '(', ')', '[', ']', '{', '}',
            '<', '>', '|', '\\', '/', '@', '#',
            '$', '%', '^', '&', '*', '_', '-',
            '+', '=', '~', '`'
        };

        // Act & Assert
        foreach (var punct in asciiPunctuation)
        {
            TextPunctuation.IsPunctuation(punct).Should().BeTrue(
                because: $"'{punct}' is standard ASCII punctuation");
        }
    }

    [Test]
    public void IsPunctuation_ArabicPunctuation_ShouldReturnTrue()
    {
        // Arrange - Arabic-specific punctuation
        char[] arabicPunctuation = {
            '،', // Arabic Comma (U+060C)
            '؛', // Arabic Semicolon (U+061B)
            '؟'  // Arabic Question Mark (U+061F)
        };

        // Act & Assert
        foreach (var punct in arabicPunctuation)
        {
            TextPunctuation.IsPunctuation(punct).Should().BeTrue(
                because: $"'{punct}' (U+{((int)punct):X4}) is Arabic punctuation");
        }
    }

    [Test]
    public void IsPunctuation_UnicodePunctuation_ShouldReturnTrue()
    {
        // Arrange - Extended Unicode punctuation
        char[] unicodePunctuation = {
            '¡', // Inverted Exclamation (U+00A1)
            '¿', // Inverted Question (U+00BF)
            '÷', // Division Sign (U+00F7)
            '×'  // Multiplication Sign (U+00D7)
        };

        // Act & Assert
        foreach (var punct in unicodePunctuation)
        {
            TextPunctuation.IsPunctuation(punct).Should().BeTrue(
                because: $"'{punct}' (U+{((int)punct):X4}) is Unicode punctuation");
        }
    }

    [Test]
    public void IsPunctuation_ArabicLetters_ShouldReturnFalse()
    {
        // Arrange
        char[] arabicLetters = { 'ا', 'ب', 'ت', 'ك', 'م' };

        // Act & Assert
        foreach (var letter in arabicLetters)
        {
            TextPunctuation.IsPunctuation(letter).Should().BeFalse(
                because: $"'{letter}' is an Arabic letter, not punctuation");
        }
    }

    [Test]
    public void IsPunctuation_EnglishLetters_ShouldReturnFalse()
    {
        // Arrange
        char[] englishLetters = { 'A', 'Z', 'a', 'z' };

        // Act & Assert
        foreach (var letter in englishLetters)
        {
            TextPunctuation.IsPunctuation(letter).Should().BeFalse();
        }
    }

    [Test]
    public void IsPunctuation_Digits_ShouldReturnFalse()
    {
        // Arrange
        char[] digits = { '0', '1', '5', '9' };

        // Act & Assert
        foreach (var digit in digits)
        {
            TextPunctuation.IsPunctuation(digit).Should().BeFalse();
        }
    }

    [Test]
    public void IsPunctuation_Whitespace_ShouldReturnFalse()
    {
        // Arrange
        char[] whitespace = { ' ', '\t', '\n', '\r' };

        // Act & Assert
        foreach (var ws in whitespace)
        {
            TextPunctuation.IsPunctuation(ws).Should().BeFalse();
        }
    }

    #endregion

    #region HasPunctuation Tests

    [Test]
    public void HasPunctuation_TextWithPunctuation_ShouldReturnTrue()
    {
        // Arrange
        string text = "مرحبا، كيف حالك؟"; // Hello, how are you?

        // Act
        bool result = TextPunctuation.HasPunctuation(text);

        // Assert
        result.Should().BeTrue(because: "text contains Arabic comma and question mark");
    }

    [Test]
    public void HasPunctuation_TextWithoutPunctuation_ShouldReturnFalse()
    {
        // Arrange
        string text = "مرحبا بكم في المكتبة"; // Welcome to the library

        // Act
        bool result = TextPunctuation.HasPunctuation(text);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void HasPunctuation_EmptyString_ShouldReturnFalse()
    {
        // Arrange
        string text = "";

        // Act
        bool result = TextPunctuation.HasPunctuation(text);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void HasPunctuation_MixedText_ShouldReturnTrue()
    {
        // Arrange
        string text = "Hello! مرحبا"; // Contains exclamation mark

        // Act
        bool result = TextPunctuation.HasPunctuation(text);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void HasPunctuation_OnlyPunctuation_ShouldReturnTrue()
    {
        // Arrange
        string text = ".,!?؛؟";

        // Act
        bool result = TextPunctuation.HasPunctuation(text);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region RemovePunctuation(string) Tests

    [Test]
    public void RemovePunctuation_String_TextWithPunctuation_ShouldRemove()
    {
        // Arrange
        string input = "مرحبا، كيف حالك؟";
        string expected = "مرحبا كيف حالك";

        // Act
        string result = TextPunctuation.RemovePunctuation(input);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void RemovePunctuation_String_CleanText_ShouldReturnOriginal()
    {
        // Arrange - Text without punctuation
        string input = "مرحبا بكم";

        // Act
        string result = TextPunctuation.RemovePunctuation(input);

        // Assert
        result.Should().BeSameAs(input, because: "zero-allocation optimization should return original");
    }

    [Test]
    public void RemovePunctuation_String_EmptyString_ShouldReturnOriginal()
    {
        // Arrange
        string input = "";

        // Act
        string result = TextPunctuation.RemovePunctuation(input);

        // Assert
        result.Should().BeSameAs(input);
    }

    [Test]
    public void RemovePunctuation_String_NullString_ShouldReturnNull()
    {
        // Arrange
        string? input = null;

        // Act
        string? result = TextPunctuation.RemovePunctuation(input!);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void RemovePunctuation_String_MixedArabicEnglishPunctuation_ShouldRemoveAll()
    {
        // Arrange
        string input = "Hello! مرحبا، كيف؟ How are you?";
        string expected = "Hello مرحبا كيف How are you";

        // Act
        string result = TextPunctuation.RemovePunctuation(input);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void RemovePunctuation_String_OnlyPunctuation_ShouldReturnEmpty()
    {
        // Arrange
        string input = ".,!?؛؟";
        string expected = "";

        // Act
        string result = TextPunctuation.RemovePunctuation(input);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void RemovePunctuation_String_LongText_ShouldUseArrayPool()
    {
        // Arrange - Text longer than 1024 chars (triggers ArrayPool path)
        string input = string.Join("، ", Enumerable.Repeat("محمد", 300)); // ~1500 chars
        string expected = string.Join(" ", Enumerable.Repeat("محمد", 300));

        // Act
        string result = TextPunctuation.RemovePunctuation(input);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region RemovePunctuation(ReadOnlySpan, Span) Tests

    [Test]
    public void RemovePunctuation_Span_ShouldRemovePunctuation()
    {
        // Arrange
        string input = "مرحبا، كيف؟";
        Span<char> source = input.ToCharArray();
        Span<char> destination = new char[input.Length];

        // Act
        int written = TextPunctuation.RemovePunctuation(source, destination);

        // Assert
        string result = new string(destination.Slice(0, written));
        result.Should().Be("مرحبا كيف");
    }

    [Test]
    public void RemovePunctuation_Span_InPlace_ShouldWork()
    {
        // Arrange - In-place modification
        char[] buffer = "Hello!".ToCharArray();
        Span<char> span = buffer;

        // Act
        int written = TextPunctuation.RemovePunctuation(span, span);

        // Assert
        string result = new string(span.Slice(0, written));
        result.Should().Be("Hello");
    }

    [Test]
    public void RemovePunctuation_Span_BufferTooSmall_ShouldThrow()
    {
        // Arrange
        string input = "مرحبا";
        ReadOnlySpan<char> source = input;
        Span<char> destination = new char[2]; // Too small

        // Act & Assert
        bool threw = false;
        try
        {
            TextPunctuation.RemovePunctuation(source, destination);
        }
        catch (ArgumentException ex)
        {
            threw = true;
            ex.Message.Should().Contain("too small");
        }
        threw.Should().BeTrue("method should throw ArgumentException for buffer too small");
    }

    [Test]
    public void RemovePunctuation_Span_CleanText_ShouldCopyAll()
    {
        // Arrange
        string input = "مرحبا";
        Span<char> source = input.ToCharArray();
        Span<char> destination = new char[input.Length];

        // Act
        int written = TextPunctuation.RemovePunctuation(source, destination);

        // Assert
        written.Should().Be(input.Length);
        new string(destination.Slice(0, written)).Should().Be(input);
    }

    [Test]
    public void RemovePunctuation_Span_EmptySpan_ShouldReturnZero()
    {
        // Arrange
        ReadOnlySpan<char> source = ReadOnlySpan<char>.Empty;
        Span<char> destination = new char[10];

        // Act
        int written = TextPunctuation.RemovePunctuation(source, destination);

        // Assert
        written.Should().Be(0);
    }

    #endregion
}
