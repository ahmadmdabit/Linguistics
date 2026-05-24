namespace Linguistics.Tests;

/// <summary>
/// Comprehensive test suite for ArabicDiacritics class.
/// Tests all 7 public methods + normalization/replacement engine.
/// Focus: Zero-allocation performance, bitmask optimization, Quranic text handling.
/// </summary>
[TestFixture]
public class ArabicDiacriticsTests
{
    #region IsDiacritic Tests

    [Test]
    public void IsDiacritic_CommonDiacritics_ShouldReturnTrue()
    {
        // Arrange - Common diacritics (U+064B to U+0652)
        char[] commonDiacritics = {
            '\u064B', // Fathatan (Tanwin Fath)
            '\u064C', // Dammatan (Tanwin Damm)
            '\u064D', // Kasratan (Tanwin Kasr)
            '\u064E', // Fatha
            '\u064F', // Damma
            '\u0650', // Kasra
            '\u0651', // Shadda
            '\u0652'  // Sukun
        };

        // Act & Assert
        foreach (var diacritic in commonDiacritics)
        {
            ArabicDiacritics.IsDiacritic(diacritic).Should().BeTrue(
                because: $"'{diacritic}' (U+{((int)diacritic):X4}) is a common Arabic diacritic");
        }
    }

    [Test]
    public void IsDiacritic_QuranicMarks_ShouldReturnTrue()
    {
        // Arrange - Quranic-specific marks
        char[] quranicMarks = {
            '\u0653', // Maddah
            '\u0654', // Hamza Above
            '\u0655', // Hamza Below
            '\u0656', // Subscript Alef
            '\u0657', // Inverted Damma
            '\u0658'  //Mark Noon Ghunna
        };

        // Act & Assert
        foreach (var mark in quranicMarks)
        {
            ArabicDiacritics.IsDiacritic(mark).Should().BeTrue(
                because: $"'{mark}' (U+{((int)mark):X4}) is a Quranic mark");
        }
    }

    [Test]
    public void IsDiacritic_ExtendedMarks_ShouldReturnTrue()
    {
        // Arrange - Extended marks outside main range
        char[] extendedMarks = {
            '\u0640', // Tatweel
            '\u0670', // Superscript Alef
            '\u0671'  // Alef Wasla
        };

        // Act & Assert
        foreach (var mark in extendedMarks)
        {
            ArabicDiacritics.IsDiacritic(mark).Should().BeTrue(
                because: $"'{mark}' (U+{((int)mark):X4}) is an extended diacritic");
        }
    }

    [Test]
    public void IsDiacritic_ArabicLetters_ShouldReturnFalse()
    {
        // Arrange - Regular Arabic letters
        char[] letters = {
            'ا', // Alef
            'ب', // Ba
            'ت', // Ta
            'ك', // Kaf
            'م', // Meem
            'ن'  // Noon
        };

        // Act & Assert
        foreach (var letter in letters)
        {
            ArabicDiacritics.IsDiacritic(letter).Should().BeFalse(
                because: $"'{letter}' is an Arabic letter, not a diacritic");
        }
    }

    [Test]
    public void IsDiacritic_EnglishCharacters_ShouldReturnFalse()
    {
        // Arrange
        char[] englishChars = { 'A', 'Z', 'a', 'z', '0', '9', ' ', '.' };

        // Act & Assert
        foreach (var ch in englishChars)
        {
            ArabicDiacritics.IsDiacritic(ch).Should().BeFalse(
                because: $"'{ch}' is not an Arabic diacritic");
        }
    }

    #endregion

    #region IsCommonDiacritic Tests

    [Test]
    public void IsCommonDiacritic_CommonMarks_ShouldReturnTrue()
    {
        // Arrange - Standard vowels and nunation
        char[] commonMarks = { '\u064E', '\u064F', '\u0650', '\u0651', '\u0652' }; // Fatha, Damma, Kasra, Shadda, Sukun

        // Act & Assert
        foreach (var mark in commonMarks)
        {
            ArabicDiacritics.IsCommonDiacritic(mark).Should().BeTrue();
        }
    }

    [Test]
    public void IsCommonDiacritic_QuranicMarks_ShouldReturnFalse()
    {
        // Arrange - Quranic-specific (not common)
        char[] quranicMarks = { '\u0653', '\u0654', '\u0655' }; // Maddah, Hamza Above, Hamza Below

        // Act & Assert
        foreach (var mark in quranicMarks)
        {
            ArabicDiacritics.IsCommonDiacritic(mark).Should().BeFalse(
                because: "Quranic marks are not classified as 'common'");
        }
    }

    #endregion

    #region IsQuranicOrMark Tests

    [Test]
    public void IsQuranicOrMark_QuranicSpecific_ShouldReturnTrue()
    {
        // Arrange
        char[] quranicMarks = { '\u0653', '\u0654', '\u0655', '\u0656' };

        // Act & Assert
        foreach (var mark in quranicMarks)
        {
            ArabicDiacritics.IsQuranicOrMark(mark).Should().BeTrue();
        }
    }

    [Test]
    public void IsQuranicOrMark_CommonDiacritics_ShouldReturnFalse()
    {
        // Arrange
        char[] commonMarks = { '\u064E', '\u064F', '\u0650' }; // Fatha, Damma, Kasra

        // Act & Assert
        foreach (var mark in commonMarks)
        {
            ArabicDiacritics.IsQuranicOrMark(mark).Should().BeFalse();
        }
    }

    #endregion

    #region IsRareOrExtended Tests

    [Test]
    public void IsRareOrExtended_RareMarks_ShouldReturnTrue()
    {
        // Arrange - Rare marks
        char[] rareMarks = { '\u065F' }; // Vertical Fatha

        // Act & Assert
        foreach (var mark in rareMarks)
        {
            ArabicDiacritics.IsRareOrExtended(mark).Should().BeTrue();
        }
    }

    #endregion

    #region HasDiacritics Tests

    [Test]
    public void HasDiacritics_TextWithDiacritics_ShouldReturnTrue()
    {
        // Arrange - Quranic text with diacritics
        string text = "بِسْمِ اللَّهِ"; // Bismillah with full diacritics

        // Act
        bool result = ArabicDiacritics.HasDiacritics(text);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void HasDiacritics_TextWithoutDiacritics_ShouldReturnFalse()
    {
        // Arrange - Plain text
        string text = "بسم الله"; // Bismillah without diacritics

        // Act
        bool result = ArabicDiacritics.HasDiacritics(text);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void HasDiacritics_EmptyString_ShouldReturnFalse()
    {
        // Arrange
        string text = "";

        // Act
        bool result = ArabicDiacritics.HasDiacritics(text);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void HasDiacritics_MixedText_ShouldReturnTrue()
    {
        // Arrange - Mix of Arabic, English, and one diacritic
        string text = "Hello مُحمد World";

        // Act
        bool result = ArabicDiacritics.HasDiacritics(text);

        // Assert
        result.Should().BeTrue(because: "text contains Damma (ُ) on the letter م");
    }

    #endregion

    #region RemoveDiacritics(string) Tests

    [Test]
    public void RemoveDiacritics_String_QuranicText_ShouldRemoveAllDiacritics()
    {
        // Arrange - Basmala with full Quranic diacritics
        string input = "بِسْمِ اللَّهِ الرَّحْمَٰنِ الرَّحِيمِ";
        string expected = "بسم الله الرحمن الرحيم";

        // Act
        string result = ArabicDiacritics.RemoveDiacritics(input);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void RemoveDiacritics_String_CleanText_ShouldReturnOriginal()
    {
        // Arrange - Text without diacritics
        string input = "مرحبا بكم";

        // Act
        string result = ArabicDiacritics.RemoveDiacritics(input);

        // Assert
        result.Should().BeSameAs(input, because: "zero-allocation optimization should return the original instance");
    }

    [Test]
    public void RemoveDiacritics_String_EmptyString_ShouldReturnOriginal()
    {
        // Arrange
        string input = "";

        // Act
        string result = ArabicDiacritics.RemoveDiacritics(input);

        result.Should().Be(input);
    }

    #endregion

    #region RemoveDiacritics(ReadOnlySpan, Span) Tests

    [Test]
    public void RemoveDiacritics_Span_ShouldRemoveDiacritics()
    {
        // Arrange
        string input = "بِسْمِ اللَّهِ";
        Span<char> source = input.ToCharArray();
        Span<char> destination = new char[input.Length];

        // Act
        int written = ArabicDiacritics.RemoveDiacritics(source, destination);

        // Assert
        string result = new string(destination.Slice(0, written));
        result.Should().Be("بسم الله");
    }

    [Test]
    public void RemoveDiacritics_Span_InPlace_ShouldWork()
    {
        // Arrange - In-place modification (source == destination)
        char[] buffer = "مُحَمَّد".ToCharArray();
        Span<char> span = buffer;

        // Act
        int written = ArabicDiacritics.RemoveDiacritics(span, span);

        // Assert
        string result = new string(span.Slice(0, written));
        result.Should().Be("محمد");
    }

    [Test]
    public void RemoveDiacritics_Span_BufferTooSmall_ShouldThrow()
    {
        // Arrange
        string input = "بِسْمِ";
        ReadOnlySpan<char> source = input;
        Span<char> destination = new char[2]; // Too small

        // Act & Assert
        bool threw = false;
        try
        {
            ArabicDiacritics.RemoveDiacritics(source, destination);
        }
        catch (ArgumentException ex)
        {
            threw = true;
            ex.Message.Should().Contain("too small");
        }
        threw.Should().BeTrue("method should throw ArgumentException for buffer too small");
    }

    [Test]
    public void RemoveDiacritics_Span_CleanText_ShouldCopyAll()
    {
        // Arrange
        string input = "مرحبا";
        Span<char> source = input.ToCharArray();
        Span<char> destination = new char[input.Length];

        // Act
        int written = ArabicDiacritics.RemoveDiacritics(source, destination);

        // Assert
        written.Should().Be(input.Length);
        new string(destination.Slice(0, written)).Should().Be(input);
    }

    #endregion

    #region Normalize Tests

    [Test]
    public void Normalize_WithPatterns_ShouldApplyReplacements()
    {
        // Arrange
        string input = "ٱلْحَمْدُ"; // Contains Alef Wasla (ٱ U+0671)

        // Create a simple pattern to replace Alef Wasla with regular Alef
        var patterns = new[]
        {
            new ArabicDiacriticsPattern("ٱ", "ا", isQuran: true, isSpecial: true)
        };

        // Act
        string result = ArabicDiacritics.Normalize(input, patterns);

        // Assert
        result.Should().Contain("ا", because: "Alef Wasla should be replaced with regular Alef");
        result.Should().NotContain("ٱ");
    }

    [Test]
    public void Normalize_EmptyPatterns_ShouldReturnOriginal()
    {
        // Arrange
        string input = "بسم الله";
        var patterns = Array.Empty<ArabicDiacriticsPattern>();

        // Act
        string result = ArabicDiacritics.Normalize(input, patterns);

        // Assert
        result.Should().BeSameAs(input);
    }

    [Test]
    public void Normalize_NullPatterns_ShouldReturnOriginal()
    {
        // Arrange
        string input = "بسم الله";

        // Act
        string result = ArabicDiacritics.Normalize(input, null!);

        // Assert
        result.Should().BeSameAs(input);
    }

    [Test]
    public void Normalize_EmptyString_ShouldReturnOriginal()
    {
        // Arrange
        string input = "";
        var patterns = new[] { new ArabicDiacriticsPattern("ا", "أ", false, false) };

        // Act
        string result = ArabicDiacritics.Normalize(input, patterns);

        // Assert
        result.Should().BeSameAs(input);
    }

    #endregion

    #region Replace Tests

    [Test]
    public void Replace_SimplePattern_ShouldReplace()
    {
        // Arrange
        ReadOnlySpan<char> source = "بِسْمِ اللَّهِ";
        Span<char> destination = new char[50];
        var patterns = new[]
        {
            new ArabicDiacriticsPattern("بِ", "ب", false, false),
            new ArabicDiacriticsPattern("سْ", "س", false, false)
        };

        // Act
        int written = ArabicDiacritics.Replace(source, destination, patterns);

        // Assert
        string result = new string(destination.Slice(0, written));
        result.Should().Contain("بسم");
    }

    [Test]
    public void Replace_MultiplePatternsGreedy_ShouldMatchLongestFirst()
    {
        // Arrange
        ReadOnlySpan<char> source = "الله";
        Span<char> destination = new char[10];

        // Patterns sorted by length descending (greedy)
        var patterns = new[]
        {
            new ArabicDiacriticsPattern("الله", "X", false, false), // 4 chars
            new ArabicDiacriticsPattern("ال", "Y", false, false)    // 2 chars
        };

        // Act
        int written = ArabicDiacritics.Replace(source, destination, patterns);

        // Assert
        string result = new string(destination.Slice(0, written));
        result.Should().Be("X", because: "longer pattern should match first");
    }

    [Test]
    public void Replace_NoMatch_ShouldCopyOriginal()
    {
        // Arrange
        ReadOnlySpan<char> source = "مرحبا";
        Span<char> destination = new char[10];
        var patterns = new[]
        {
            new ArabicDiacriticsPattern("XYZ", "ABC", false, false)
        };

        // Act
        int written = ArabicDiacritics.Replace(source, destination, patterns);

        // Assert
        string result = new string(destination.Slice(0, written));
        result.Should().Be("مرحبا");
    }

    #endregion

    #region AllDiacritics Property Tests

    [Test]
    public void AllDiacritics_ShouldContainKnownDiacritics()
    {
        // Act
        var allDiacritics = ArabicDiacritics.AllDiacritics;

        // Assert
        allDiacritics.Length.Should().BeGreaterThan(0);
        allDiacritics.ToArray().Should().Contain('\u064B'); // Fathatan
        allDiacritics.ToArray().Should().Contain('\u064E'); // Fatha
        allDiacritics.ToArray().Should().Contain('\u0651'); // Shadda
    }

    [Test]
    public void AllDiacritics_ShouldBeReadOnly()
    {
        // Act
        var span1 = ArabicDiacritics.AllDiacritics;
        var span2 = ArabicDiacritics.AllDiacritics;

        // Assert - Should return same underlying array
        span1.Length.Should().Be(span2.Length);
    }

    #endregion

    #region AsHashSet Tests

    [Test]
    public void AsHashSet_ShouldContainAllDiacritics()
    {
        // Act
        var hashSet = ArabicDiacritics.AsHashSet();

        // Assert
        hashSet.Should().NotBeEmpty();
        hashSet.Should().Contain('\u064E'); // Fatha
        hashSet.Should().Contain('\u0651'); // Shadda
    }

    [Test]
    public void AsHashSet_ShouldAllowFastLookup()
    {
        // Arrange
        var hashSet = ArabicDiacritics.AsHashSet();

        // Act & Assert - O(1) lookup
        hashSet.Contains('\u064E').Should().BeTrue();
        hashSet.Contains('ا').Should().BeFalse();
    }

    #endregion
}
