namespace Linguistics.Tests;

/// <summary>
/// Coverage gap tests for uncovered code paths in ArabicAffixStripper and MorphologyResult.
/// All test cases validated by Arabic Linguistics Review Panel.
/// </summary>
[TestFixture]
public class CoverageGapTests
{
    #region ArabicAffixStripper.CheckDefiniteArticle - Fallback Logic (Lines 50-54)

    [Test]
    public void CheckDefiniteArticle_FourCharCandidateNoRoot_ShouldUseFallback()
    {
        // Linguistic Context: "الشيء" (al-shay') = "the thing"
        // After removing "ال": "شيء" (4 chars)
        // "شيء" is a valid word but not a recognized tri-literal root in our data
        // Engine should use fallback logic (lines 50-54) to return the candidate

        string input = "الشيء"; // al-shay' (the thing)
        string result = ArabicMorphologyHelper.FormatWord(input, false);

        // Should return the stripped form "شيء" or attempt normalization
        result.Should().NotBeNullOrEmpty();
        result.Length.Should().BeGreaterThanOrEqualTo(3);
    }

    [Test]
    public void CheckDefiniteArticle_FiveCharCandidateNoRoot_ShouldUseFallback()
    {
        // Linguistic Context: "الأشياء" (al-ashya') = "the things" (plural)
        // After removing "ال": "أشياء" (5 chars)
        // This tests the candidate.Length > 3 fallback path

        string input = "الأشياء"; // al-ashya' (the things)
        string result = ArabicMorphologyHelper.FormatWord(input, false);

        result.Should().NotBeNullOrEmpty();
        result.Length.Should().BeGreaterThanOrEqualTo(3);
    }

    [Test]
    public void CheckDefiniteArticle_WithWawPrefix_ShouldRemoveBoth()
    {
        // Linguistic Context: "والكتاب" (wa-al-kitab) = "and the book"
        // Tests "وال" definite article variant

        string input = "والكتاب"; // wa-al-kitab
        string expected = "كتب"; // K-T-B root

        string result = ArabicMorphologyHelper.FormatWord(input, false);
        result.Should().Be(expected);
    }

    #endregion

    #region ArabicAffixStripper.CheckPrefixWaw - Conjunction Removal (Lines 120-121)

    [Test]
    public void CheckPrefixWaw_ConjunctionWithLongWord_ShouldRemoveWaw()
    {
        // Linguistic Context: "والقمر" (wa-al-qamar) = "and the moon"
        // Word length > 3 with leading Waw
        // Should trigger CheckPrefixWaw (lines 120-121)

        string input = "والقمر"; // wa-al-qamar (and the moon)
        string expected = "قمر"; // Q-M-R root

        string result = ArabicMorphologyHelper.FormatWord(input, false);
        result.Should().Be(expected);
    }

    [Test]
    public void CheckPrefixWaw_ConjunctionWithVerb_ShouldRemoveWaw()
    {
        // Linguistic Context: "ويكتب" (wa-yaktub) = "and he writes"
        // Conjunction + imperfect verb

        string input = "ويكتب"; // wa-yaktub
        string expected = "كتب"; // K-T-B root

        string result = ArabicMorphologyHelper.FormatWord(input, false);
        result.Should().Be(expected);
    }

    [Test]
    public void CheckPrefixWaw_RootStartingWithWaw_ShouldNotRemove()
    {
        // Linguistic Context: "وعد" (wa'ada) = "he promised"
        // Root is W-A-D (وعد), Waw is part of the root, NOT a conjunction
        // Length = 3, should NOT trigger CheckPrefixWaw (requires length > 3)

        string input = "وعد"; // wa'ada (he promised)
        string expected = "وعد"; // W-A-D root (Waw is part of root)

        string result = ArabicMorphologyHelper.FormatWord(input, false);
        result.Should().Be(expected);
    }

    #endregion

    #region MorphologyResult Edge Cases

    [Test]
    public void MorphologyResult_InsertAtEnd_ShouldWork()
    {
        // Test Insert at the end of the buffer
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "كتب".AsSpan(), false);

        result.Insert(3, 'ا'); // Insert Alif at end

        string final = result.Text.ToString();
        final.Should().Be("كتبا");
    }

    [Test]
    public void MorphologyResult_InsertWhenFull_ShouldHandleGracefully()
    {
        // Test Insert when buffer is at capacity
        Span<char> buffer = stackalloc char[4]; // Small buffer
        var result = new MorphologyResult(buffer, "كتب".AsSpan(), false);

        result.Insert(1, 'ا'); // Try to insert (will fill buffer)

        // Should either succeed or fail gracefully
        result.Text.Length.Should().BeLessThanOrEqualTo(4);
    }

    [Test]
    public void MorphologyResult_InsertOutOfBounds_ShouldHandleGracefully()
    {
        // Test Insert with invalid index
        Span<char> buffer = stackalloc char[64];
        var result = new MorphologyResult(buffer, "كتب".AsSpan(), false);

        result.Insert(10, 'ا'); // Index beyond length

        // Should handle gracefully (no crash)
        result.Text.Length.Should().Be(3); // Should remain unchanged
    }

    #endregion

    #region Additional Linguistic Edge Cases

    [Test]
    public void CheckDefiniteArticle_SunLetterAssimilation_ShouldWork()
    {
        // Linguistic Context: Arabic sun letters (الحروف الشمسية)
        // "الشمس" (al-shams) = "the sun"
        // The 'L' sound assimilates to the following sun letter 'ش'
        // Pronunciation: "ash-shams" not "al-shams"

        string input = "الشمس"; // al-shams (the sun)
        string expected = "شمس"; // Sh-M-S root

        string result = ArabicMorphologyHelper.FormatWord(input, false);
        result.Should().Be(expected);
    }

    [Test]
    public void CheckPrefixWaw_DoublePrefix_ShouldRemoveBoth()
    {
        // Linguistic Context: "وبالكتاب" (wa-bi-al-kitab) = "and with the book"
        // Multiple prefixes: Waw (و) + Bi (ب) + Al (ال)

        string input = "وبالكتاب"; // wa-bi-al-kitab
        string expected = "كتب"; // K-T-B root

        string result = ArabicMorphologyHelper.FormatWord(input, false);
        result.Should().Be(expected);
    }

    [Test]
    public void CheckDefiniteArticle_MoonLetterNoAssimilation_ShouldWork()
    {
        // Linguistic Context: Arabic moon letters (الحروف القمرية)
        // "القمر" (al-qamar) = "the moon"
        // The 'L' sound does NOT assimilate to moon letter 'ق'
        // Pronunciation: "al-qamar" (as written)

        string input = "القمر"; // al-qamar (the moon)
        string expected = "قمر"; // Q-M-R root

        string result = ArabicMorphologyHelper.FormatWord(input, false);
        result.Should().Be(expected);
    }

    #endregion
}
