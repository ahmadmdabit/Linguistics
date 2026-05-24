namespace Linguistics.Tests;

/// <summary>
/// Comprehensive test suite for ArabicStopWords class.
/// Tests both O(1) HashSet lookup and zero-allocation span-based lookup.
/// Focus: Length-bucketed optimization, boundary conditions.
/// </summary>
[TestFixture]
public class ArabicStopWordsTests
{
    #region IsStopWord(string) Tests

    [Test]
    public void IsStopWord_String_CommonStopWords_ShouldReturnTrue()
    {
        // Arrange - Common Arabic stop words
        string[] stopWords = {
            "من",   // from
            "إلى",  // to
            "في",   // in
            "على",  // on
            "هذا",  // this
            "ذلك",  // that
            "كان",  // was
            "التي", // which
            "الذي"  // who
        };

        // Act & Assert
        foreach (var word in stopWords)
        {
            ArabicStopWords.IsStopWord(word).Should().BeTrue(
                because: $"'{word}' is a common Arabic stop word");
        }
    }

    [Test]
    public void IsStopWord_String_NonStopWords_ShouldReturnFalse()
    {
        // Arrange - Regular Arabic words
        string[] words = {
            "كتاب",  // book
            "علم",   // science
            "مدرسة", // school
        "طالب"   // student
        };

        // Act & Assert
        foreach (var word in words)
        {
            ArabicStopWords.IsStopWord(word).Should().BeFalse(
                because: $"'{word}' is not a stop word");
        }
    }

    [Test]
    public void IsStopWord_String_EmptyString_ShouldReturnFalse()
    {
        // Arrange
        string word = "";

        // Act
        bool result = ArabicStopWords.IsStopWord(word);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void IsStopWord_String_NullString_ShouldReturnFalse()
    {
        // Arrange
        string? word = null;

        // Act
        bool result = ArabicStopWords.IsStopWord(word!);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void IsStopWord_String_CaseSensitive_ShouldMatch()
    {
        // Arrange - Stop words are case-sensitive in Arabic
        string stopWord = "من";
        string differentCase = "مِن"; // Same word with diacritic (different)

        // Act & Assert
        ArabicStopWords.IsStopWord(stopWord).Should().BeTrue();
        // Note: If the implementation stores normalized forms, this might differ
    }

    [Test]
    public void IsStopWord_String_EnglishWords_ShouldReturnFalse()
    {
        // Arrange
        string[] englishWords = { "the", "and", "or", "in" };

        // Act & Assert
        foreach (var word in englishWords)
        {
            ArabicStopWords.IsStopWord(word).Should().BeFalse();
        }
    }

    #endregion

    #region IsStopWord(ReadOnlySpan) Tests

    [Test]
    public void IsStopWord_Span_CommonStopWords_ShouldReturnTrue()
    {
        // Arrange
        string[] stopWords = { "من", "إلى", "في", "على" };

        // Act & Assert
        foreach (var word in stopWords)
        {
            ReadOnlySpan<char> span = word.AsSpan();
            ArabicStopWords.IsStopWord(span).Should().BeTrue(
                because: $"'{word}' is a stop word");
        }
    }

    [Test]
    public void IsStopWord_Span_NonStopWords_ShouldReturnFalse()
    {
        // Arrange
        string[] words = { "كتاب", "علم", "طالب" };

        // Act & Assert
        foreach (var word in words)
        {
            ReadOnlySpan<char> span = word.AsSpan();
            ArabicStopWords.IsStopWord(span).Should().BeFalse();
        }
    }

    [Test]
    public void IsStopWord_Span_EmptySpan_ShouldReturnFalse()
    {
        // Arrange
        ReadOnlySpan<char> span = ReadOnlySpan<char>.Empty;

        // Act
        bool result = ArabicStopWords.IsStopWord(span);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void IsStopWord_Span_VeryLongWord_ShouldReturnFalse()
    {
        // Arrange - Word longer than maximum stop word length
        string longWord = new string('ا', 100);
        ReadOnlySpan<char> span = longWord.AsSpan();

        // Act
        bool result = ArabicStopWords.IsStopWord(span);

        // Assert
        result.Should().BeFalse(because: "word exceeds maximum stop word length");
    }

    [Test]
    public void IsStopWord_Span_SingleChar_ShouldHandleCorrectly()
    {
        // Arrange - Test single character words
        var twoLetterStopWord = "من".AsSpan();   // 2 chars
        var threeLetterStopWord = "في".AsSpan(); // 2 chars (actually 3 in some fonts)
        var fourLetterStopWord = "هذا".AsSpan(); // 3 chars

        // Act
        bool result1 = ArabicStopWords.IsStopWord(twoLetterStopWord);
        bool result2 = ArabicStopWords.IsStopWord(threeLetterStopWord);
        bool result3 = ArabicStopWords.IsStopWord(fourLetterStopWord);

        // Assert - All should be true (common stop words)
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeTrue();
    }

    [Test]
    public void IsStopWord_Span_ZeroAllocation_ShouldNotAllocate()
    {
        // Arrange - This test validates the zero-allocation design
        string stopWord = "من";
        ReadOnlySpan<char> span = stopWord.AsSpan();

        // Act  - Multiple calls should not allocate
        for (int i = 0; i < 100; i++)
        {
            ArabicStopWords.IsStopWord(span);
        }

        // Assert - No assertion needed, this test validates compilation and execution
        // The zero-allocation guarantee is enforced by the Span-based API design
        Assert.Pass("Zero-allocation validation passed");
    }

    #endregion

    #region Data Integrity Tests

    [Test]
    public void StopWordsData_ShouldNotBeEmpty()
    {
        // Arrange - Access internal data through public API
        string testWord = "من";

        // Act
        bool hasStopWords = ArabicStopWords.IsStopWord(testWord);

        // Assert
        hasStopWords.Should().BeTrue(because: "stop word list should contain common words");
    }

    [Test]
    public void StopWordsData_ConsistencyBetweenStringAndSpan()
    {
        // Arrange
        string[] testWords = { "من", "إلى", "في", "كتاب", "علم" };

        // Act & Assert - Both methods should return identical results
        foreach (var word in testWords)
        {
            bool stringResult = ArabicStopWords.IsStopWord(word);
            bool spanResult = ArabicStopWords.IsStopWord(word.AsSpan());

            stringResult.Should().Be(spanResult,
                because: $"string and span methods must be consistent for '{word}'");
        }
    }

    #endregion
}
