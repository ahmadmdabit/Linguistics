namespace Linguistics.Tests;

/// <summary>
/// Targeted tests for the ArabicAffixStripper class.
/// Focuses on prefix, suffix, and definite article removal.
/// </summary>
[TestFixture]
public class ArabicAffixStripperTests
{
    // Helper to create a MorphologyResult for testing
    private static string StripAffixes(string input)
    {
        // Note: ArabicAffixStripper relies on recursion back to ArabicMorphologyHelper.AnalyzeCandidate.
        // So we can't just call "CheckPrefixes" in isolation without the whole pipeline potentially firing.
        // However, we can test that it *starts* the process.

        // Actually, since we want to test the *Logic* of the stripper, and the stripper calls AnalyzeCandidate,
        // the end result should be the extracted root if successful.

        // So effectively, testing AffixStripper is testing the "Affix -> Root" path.

        return ArabicMorphologyHelper.FormatWord(input, false);
    }

    [Test]
    public void CheckDefiniteArticle_ShouldRemoveAl()
    {
        // "الكتاب" -> "كتب"
        StripAffixes("الكتاب").Should().Be("كتب");
    }

    [Test]
    public void CheckPrefixes_ShouldRemovePrepositions()
    {
        // "بالمدرسة" (Bi-Al-Madrasa) -> "درس"
        // Prefix 'Bi', then Article 'Al'
        StripAffixes("بالمدرسة").Should().Be("درس");
    }

    [Test]
    public void CheckSuffixes_ShouldRemovePronouns()
    {
        // "كتابهم" (Kitabuhum) -> "كتب"
        StripAffixes("كتابهم").Should().Be("كتب");
    }

    [Test]
    public void CheckComplexAffixes_TheLongestWord_ShouldExtractRoot()
    {
        // "فسيكفيكهم" (Fa-Sa-Yakfeekahum) -> "كفي"
        // This was the failing test case in the previous run.
        // Now that we have better isolation (or at least better structure), let's see if it passes.
        // If logic is the same, it might still fail, but now we can debug it better.

        StripAffixes("فسيكفيكهم").Should().Be("كفي");
    }

    [Test]
    public void CheckPrefixWaw_ShouldHandleConjunction()
    {
        // "والقمر" -> "قمر"
        StripAffixes("والقمر").Should().Be("قمر");
    }
}
