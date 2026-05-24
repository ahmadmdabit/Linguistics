using Linguistics.Data;

namespace Linguistics.Tests;

/// <summary>
/// Test suite for ArabicDiacriticsPattern struct.
/// Tests pattern construction, caching, and optimization features.
/// Focus: StartChar optimization, length caching, pattern matching behavior.
/// </summary>
[TestFixture]
public class ArabicDiacriticsPatternTests
{
    #region Constructor Tests

    [Test]
    public void Constructor_ValidPattern_ShouldInitialize()
    {
        // Arrange
        string unicode = "بِسْمِ";
        string replacement = "بسم";

        // Act
        var pattern = new ArabicDiacriticsPattern(unicode, replacement, isQuran: true, isSpecial: false);

        // Assert
        pattern.Unicode.Should().Be(unicode);
        pattern.Replacement.Should().Be(replacement);
        pattern.IsQuran.Should().BeTrue();
        pattern.IsSpecial.Should().BeFalse();
    }

    [Test]
    public void Constructor_EmptyPatterns_ShouldWork()
    {
        // Arrange & Act
        var pattern = new ArabicDiacriticsPattern("", "", isQuran: false, isSpecial: false);

        // Assert
        pattern.Unicode.Should().BeEmpty();
        pattern.Replacement.Should().BeEmpty();
    }

    #endregion

    #region Length Caching Tests

    [Test]
    public void Length_ShouldCacheUnicodeLength()
    {
        // Arrange
        string unicode = "بِسْمِ";
        var pattern = new ArabicDiacriticsPattern(unicode, "", isQuran: true, isSpecial: false);

        // Act
        int length = pattern.Length;

        // Assert
        length.Should().Be(unicode.Length);
    }

    [Test]
    public void Length_EmptyString_ShouldBeZero()
    {
        // Arrange
        var pattern = new ArabicDiacriticsPattern("", "", isQuran: false, isSpecial: false);

        // Act
        int length = pattern.Length;

        // Assert
        length.Should().Be(0);
    }

    #endregion

    #region StartChar Optimization Tests

    [Test]
    public void StartChar_ShouldCacheFirstCharacter()
    {
        // Arrange
        string unicode = "بِسْمِ";
        var pattern = new ArabicDiacriticsPattern(unicode, "", isQuran: true, isSpecial: false);

        // Act
        char startChar = pattern.StartChar;

        // Assert
        startChar.Should().Be('ب');
    }

    [Test]
    public void StartChar_EmptyString_ShouldBeNull()
    {
        // Arrange
        var pattern = new ArabicDiacriticsPattern("", "", isQuran: false, isSpecial: false);

        // Act
        char startChar = pattern.StartChar;

        // Assert
        startChar.Should().Be(ArabicConstantsChar.Null);
    }

    [Test]
    public void StartChar_SingleChar_ShouldMatchUnicode()
    {
        // Arrange
        string unicode = "ب";
        var pattern = new ArabicDiacriticsPattern(unicode, "", isQuran: false, isSpecial: false);

        // Act
        char startChar = pattern.StartChar;

        // Assert
        startChar.Should().Be('ب');
        pattern.Length.Should().Be(1);
    }

    #endregion

    #region Pattern Flags Tests

    [Test]
    public void IsQuran_WhenTrue_ShouldBeSet()
    {
        // Arrange & Act
        var pattern = new ArabicDiacriticsPattern("test", "test", isQuran: true, isSpecial: false);

        // Assert
        pattern.IsQuran.Should().BeTrue();
    }

    [Test]
    public void IsQuran_WhenFalse_ShouldNotBeSet()
    {
        // Arrange & Act
        var pattern = new ArabicDiacriticsPattern("test", "test", isQuran: false, isSpecial: false);

        // Assert
        pattern.IsQuran.Should().BeFalse();
    }

    [Test]
    public void IsSpecial_WhenTrue_ShouldBeSet()
    {
        // Arrange & Act
        var pattern = new ArabicDiacriticsPattern("test", "test", isQuran: false, isSpecial: true);

        // Assert
        pattern.IsSpecial.Should().BeTrue();
    }

    [Test]
    public void IsSpecial_WhenFalse_ShouldNotBeSet()
    {
        // Arrange & Act
        var pattern = new ArabicDiacriticsPattern("test", "test", isQuran: false, isSpecial: false);

        // Assert
        pattern.IsSpecial.Should().BeFalse();
    }

    #endregion

    #region Replacement Tests

    [Test]
    public void Replacement_ShouldStoreValue()
    {
        // Arrange
        string replacement = "الله";
        var pattern = new ArabicDiacriticsPattern("test", replacement, isQuran: true, isSpecial: false);

        // Act & Assert
        pattern.Replacement.Should().Be(replacement);
    }

    [Test]
    public void Replacement_EmptyString_ShouldWork()
    {
        // Arrange - Empty replacement means removal
        var pattern = new ArabicDiacriticsPattern("test", "", isQuran: false, isSpecial: false);

        // Act & Assert
        pattern.Replacement.Should().BeEmpty();
    }

    #endregion

    #region Pattern Matching Behavior Tests

    [Test]
    public void Pattern_AfterConstruction_AllPropertiesShouldBeAccessible()
    {
        // Arrange
        string unicode = "بِسْمِ";
        string replacement = "بسم";
        bool isQuran = true;
        bool isSpecial = false;

        // Act
        var pattern = new ArabicDiacriticsPattern(unicode, replacement, isQuran, isSpecial);

        // Assert - All properties should be accessible
        pattern.Unicode.Should().Be(unicode);
        pattern.Replacement.Should().Be(replacement);
        pattern.IsQuran.Should().Be(isQuran);
        pattern.IsSpecial.Should().Be(isSpecial);
        pattern.Length.Should().Be(unicode.Length);
        pattern.StartChar.Should().Be(unicode[0]);
    }

    [Test]
    public void Pattern_QuranicExample_ShouldWorkCorrectly()
    {
        // Arrange - Real Quranic normalization example
        // Alef Wasla (ٱ U+0671) -> Regular Alef (ا U+0627)
        string unicode = "ٱ";
        string replacement = "ا";
        var pattern = new ArabicDiacriticsPattern(unicode, replacement, isQuran: true, isSpecial: true);

        // Act & Assert
        pattern.Unicode.Should().Be("ٱ");
        pattern.Replacement.Should().Be("ا");
        pattern.IsQuran.Should().BeTrue();
        pattern.IsSpecial.Should().BeTrue();
        pattern.StartChar.Should().Be('ٱ');
        pattern.Length.Should().Be(1);
    }

    [Test]
    public void Pattern_ComplexDiacriticCombination_ShouldCacheCorrectly()
    {
        // Arrange - Complex combination: Hamza + Fatha + Alef
        string unicode = "أَا"; // Complex diacritic sequence
        string replacement = "آ"; // Alef with Maddah
        var pattern = new ArabicDiacriticsPattern(unicode, replacement, isQuran: true, isSpecial: true);

        // Act & Assert
        pattern.Length.Should().Be(unicode.Length);
        pattern.StartChar.Should().Be(unicode[0]);
    }

    #endregion

    #region Struct Behavior Tests

    [Test]
    public void Pattern_IsValueType_ShouldBeCopyable()
    {
        // Arrange
        var pattern1 = new ArabicDiacriticsPattern("test", "TEST", isQuran: false, isSpecial: false);

        // Act - Struct copy semantics
        var pattern2 = pattern1;

        // Assert - Both should have same values (value type copy)
        pattern2.Unicode.Should().Be(pattern1.Unicode);
        pattern2.Replacement.Should().Be(pattern1.Replacement);
        pattern2.Length.Should().Be(pattern1.Length);
    }

    #endregion
}
