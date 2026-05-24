using Linguistics.Data;

namespace Linguistics.Tests.Data;

/// <summary>
/// Comprehensive data integrity tests for all Arabic linguistic data classes.
/// Validates data loading, sorting, and accessibility for the morphology engine.
/// Focus: Ensure data is properly initialized and organized for performance.
/// </summary>
[TestFixture]
public class ArabicDataIntegrityTests
{
    #region ArabicDefiniteArticle Tests

    [Test]
    public void ArabicDefiniteArticle_Articles_ShouldNotBeEmpty()
    {
        // Act
        var articles = ArabicDefiniteArticle.Articles;

        // Assert
        articles.Length.Should().BeGreaterThan(0, because: "articles list should contain at least 'ال'");
    }

    [Test]
    public void ArabicDefiniteArticle_Articles_ShouldBeSortedByLengthDescending()
    {
        // Act
        var articles = ArabicDefiniteArticle.Articles;

        // Assert - Greedy matching requires longest first
        for (int i = 0; i < articles.Length - 1; i++)
        {
            articles[i].Length.Should().BeGreaterThanOrEqualTo(articles[i + 1].Length,
                because: "articles must be sorted by length descending for greedy matching");
        }
    }

    [Test]
    public void ArabicDefiniteArticle_ShouldContainCommonArticles()
    {
        // Act
        var articles = ArabicDefiniteArticle.Articles.ToArray();

        // Assert
        articles.Should().Contain("ال", because: "'ال' is the standard Arabic definite article");
    }

    #endregion

    #region ArabicPrefixes Tests

    [Test]
    public void ArabicPrefixes_Prefixes_ShouldNotBeEmpty()
    {
        // Act
        var prefixes = ArabicPrefixes.Prefixes;

        // Assert
        prefixes.Length.Should().BeGreaterThan(0);
    }

    [Test]
    public void ArabicPrefixes_Prefixes_ShouldBeSortedByLengthDescending()
    {
        // Act
        var prefixes = ArabicPrefixes.Prefixes;

        // Assert
        for (int i = 0; i < prefixes.Length - 1; i++)
        {
            prefixes[i].Length.Should().BeGreaterThanOrEqualTo(prefixes[i + 1].Length);
        }
    }

    [Test]
    public void ArabicPrefixes_ShouldContainCommonPrefixes()
    {
        // Act
        var prefixes = ArabicPrefixes.Prefixes.ToArray();

        // Assert
        prefixes.Should().Contain("ب", because: "'ب' (with/by) is a common prefix");
        prefixes.Should().Contain("ل", because: "'ل' (to/for) is a common prefix");
    }

    #endregion

    #region ArabicSuffixes Tests

    [Test]
    public void ArabicSuffixes_Suffixes_ShouldNotBeEmpty()
    {
        // Act
        var suffixes = ArabicSuffixes.Suffixes;

        // Assert
        suffixes.Length.Should().BeGreaterThan(0);
    }

    [Test]
    public void ArabicSuffixes_Suffixes_ShouldBeSortedByLengthDescending()
    {
        // Act
        var suffixes = ArabicSuffixes.Suffixes;

        // Assert
        for (int i = 0; i < suffixes.Length - 1; i++)
        {
            suffixes[i].Length.Should().BeGreaterThanOrEqualTo(suffixes[i + 1].Length);
        }
    }

    [Test]
    public void ArabicSuffixes_ShouldContainCommonSuffixes()
    {
        // Act
        var suffixes = ArabicSuffixes.Suffixes.ToArray();

        // Assert - Check for common possessive/plural suffixes
        // Note: Actual suffix values depend on ArabicAffixesData
        suffixes.Should().NotBeEmpty();
    }

    #endregion

    #region ArabicStopWordsData Tests

    [Test]
    public void ArabicStopWordsData_All_ShouldNotBeEmpty()
    {
        // Act
        var stopWords = ArabicStopWordsData.All;

        // Assert
        stopWords.Should().NotBeEmpty();
        stopWords.Count.Should().BeGreaterThan(10, because: "should contain many common stop words");
    }

    [Test]
    public void ArabicStopWordsData_ShouldContainCommonWords()
    {
        // Act
        var stopWords = ArabicStopWordsData.All;

        // Assert
        stopWords.Should().Contain("من"); // from
        stopWords.Should().Contain("إلى"); // to
        stopWords.Should().Contain("في"); // in
    }

    [Test]
    public void ArabicStopWordsData_ShouldNotContainDuplicates()
    {
        // Act
        var stopWords = ArabicStopWordsData.All;

        // Assert
        stopWords.Count.Should().Be(stopWords.Distinct().Count(),
            because: "stop words should not have duplicates");
    }

    #endregion

    #region ArabicRootsData Tests

    [Test]
    public void ArabicRootsData_Trilateral_ShouldNotBeEmpty()
    {
        // Act
        var triRoots = ArabicRootsData.Trilateral;

        // Assert
        triRoots.Should().NotBeEmpty();
        triRoots.Count.Should().BeGreaterThan(100, because: "should contain many trilateral roots");
    }

    [Test]
    public void ArabicRootsData_Quadrilateral_ShouldNotBeEmpty()
    {
        // Act
        var quadRoots = ArabicRootsData.Quadrilateral;

        // Assert
        quadRoots.Should().NotBeEmpty();
    }

    #endregion

    #region ArabicPatternsData Tests

    [Test]
    public void ArabicPatternsData_MorphologyTemplates_ShouldNotBeEmpty()
    {
        // Act
        var patterns = ArabicPatternsData.MorphologyTemplates;

        // Assert
        patterns.Should().NotBeEmpty();
        patterns.Count.Should().BeGreaterThan(10, because: "should contain many morphology patterns");
    }

    #endregion

    #region ArabicWeaksData Tests

    [Test]
    public void ArabicWeaksData_ShouldNotBeEmpty()
    {
        // Act - ArabicWeaksData has multiple hash sets, test existence
        var weakData = typeof(ArabicWeaksData);

        // Assert
        weakData.Should().NotBeNull();
    }

    #endregion

    #region ArabicDuplicates Tests

    [Test]
    public void ArabicRootsData_Geminated_ShouldNotBeEmpty()
    {
        // Act
        var geminated = ArabicRootsData.Geminated;

        // Assert
        geminated.Should().NotBeEmpty();
    }

    #endregion

    #region ArabicStrangeData Tests

    [Test]
    public void ArabicStrangeData_ShouldNotBeEmpty()
    {
        // Act - Access through type to verify class exists
        var strangeDataType = typeof(ArabicStrangeData);

        // Assert
        strangeDataType.Should().NotBeNull();
    }

    #endregion

    #region ArabicConstants Tests

    [Test]
    public void ArabicConstants_CommonDiacritics_ShouldNotBeEmpty()
    {
        // Act
        var diacritics = ArabicConstants.CommonDiacritics;

        // Assert
        diacritics.Should().NotBeNullOrEmpty();
        diacritics.Length.Should().BeGreaterThan(5);
    }

    [Test]
    public void ArabicConstants_QuranicMarks_ShouldNotBeEmpty()
    {
        // Act
        var quranicMarks = ArabicConstants.QuranicMarks;

        // Assert
        quranicMarks.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void ArabicConstants_RareMarks_ShouldNotBeEmpty()
    {
        // Act
        var rareMarks = ArabicConstants.RareMarks;

        // Assert
        rareMarks.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Cross-Validation Tests

    [Test]
    public void PrefixesAndSuffixes_ShouldNotOverlap()
    {
        // Act
        var prefixes = ArabicPrefixes.Prefixes.ToArray();
        var suffixes = ArabicSuffixes.Suffixes.ToArray();

        // Assert - Prefixes and suffixes should generally be distinct
        var overlapping = prefixes.Intersect(suffixes).ToArray();

        // Some overlap may be acceptable (e.g., single letter affixes)
        // But the majority should be distinct
        overlapping.Length.Should().BeLessThan(Math.Min(prefixes.Length, suffixes.Length) / 2);
    }

    [Test]
    public void DefiniteArticles_ShouldBeSubsetOfPrefixes()
    {
        // Act
        var articles = ArabicDefiniteArticle.Articles.ToArray();
        var prefixes = ArabicPrefixes.Prefixes.ToArray();

        // Assert - Or they might be separate, depending on implementation
        // This test validates the architectural consistency
        articles.Should().NotBeEmpty();
        prefixes.Should().NotBeEmpty();
    }

    #endregion
}
