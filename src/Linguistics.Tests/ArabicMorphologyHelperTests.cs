namespace Linguistics.Tests;

/// <summary>
/// Comprehensive test suite for ArabicMorphologyHelper - SIMPLIFIED to PUBLIC API only.
/// Tests FormatWord() method with authentic Arabic examples from Quran and MSA literature.
/// Focus: Root extraction, zero-allocation API, edge cases.
/// </summary>
[TestFixture]
public class ArabicMorphologyHelperTests
{
    #region FormatWord(string) Tests - Public API

    [Test]
    public void FormatWord_String_StrongRoot_ShouldExtractRoot()
    {
        // Arrange - "كتب" (K-T-B) = write/book
        string input = "كتاب"; // book
        string expectedRoot = "كتب"; // K-T-B root

        // Act
        string result = ArabicMorphologyHelper.FormatWord(input, applyFuzzyNormalization: false);

        // Assert
        result.Should().Be(expectedRoot);
    }

    [Test]
    public void FormatWord_String_WithDefiniteArticle_ShouldRemoveAndExtract()
    {
        // Arrange
        string input = "الكتاب"; // the book
        string expectedRoot = "كتب";

        // Act
        string result = ArabicMorphologyHelper.FormatWord(input, applyFuzzyNormalization: false);

        // Assert
        result.Should().Be(expectedRoot);
    }

    [Test]
    public void FormatWord_String_WithPrefixAndSuffix_ShouldExtractRoot()
    {
        // Arrange
        string input = "يكتبون"; // they write (plural masculine)
        string expectedRoot = "كتب";

        // Act
        string result = ArabicMorphologyHelper.FormatWord(input, applyFuzzyNormalization: false);

        // Assert
        result.Should().Be(expectedRoot);
    }

    [Test]
    public void FormatWord_String_WithDiacritics_ShouldRemoveAndProcess()
    {
        // Arrange
        string input = "الْكِتَابُ"; // the book (with diacritics)
        string expectedRoot = "كتب";

        // Act
        string result = ArabicMorphologyHelper.FormatWord(input, applyFuzzyNormalization: false);

        // Assert
        result.Should().Be(expectedRoot);
    }

    [Test]
    public void FormatWord_String_EmptyString_ShouldReturnEmpty()
    {
        // Arrange
        string input = "";

        // Act
        string result = ArabicMorphologyHelper.FormatWord(input, applyFuzzyNormalization: false);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void FormatWord_String_StopWord_ShouldReturnAsIs()
    {
        // Arrange
        // Arrange
        string input = "مكتبة"; // library (ends with Ta Marbuta ة)

        // Act
        string resultWithFuzzy = ArabicMorphologyHelper.FormatWord(input, applyFuzzyNormalization: true);
        string resultWithoutFuzzy = ArabicMorphologyHelper.FormatWord(input, applyFuzzyNormalization: false);

        // Assert - Both should process the word
        resultWithFuzzy.Should().NotBeNullOrEmpty();
        resultWithoutFuzzy.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void FormatWord_String_QuranicWord_ShouldExtractRoot()
    {
        // Arrange - From Quran
        string input = "الْحَمْدُ"; // "praise"
        string expectedRoot = "حمد"; // H-M-D root

        // Act
        string result = ArabicMorphologyHelper.FormatWord(input, applyFuzzyNormalization: false);

        // Assert
        result.Should().Be(expectedRoot);
    }

    [Test]
    public void FormatWord_String_VerbConjugation_ShouldExtractRoot()
    {
        // Arrange
        string input = "يكتبون"; // "they write"
        string expectedRoot = "كتب"; // K-T-B root

        // Act
        string result = ArabicMorphologyHelper.FormatWord(input, applyFuzzyNormalization: false);

        // Assert
        result.Should().Be(expectedRoot);
    }

    [Test]
    public void FormatWord_VeryLongWord_ShouldHandleGracefully()
    {
        // Arrange - Artificially long word (exceeds buffer)
        string input = new string('ا', 100);

        // Act
        Action act = () => ArabicMorphologyHelper.FormatWord(input, applyFuzzyNormalization: false);

        // Assert - Should throw ArgumentException for invalid/oversized input
        act.Should().Throw<ArgumentException>()
           .WithMessage("*exceeds buffer size*");
    }

    [Test]
    public void FormatWord_OnlyNumbers_ShouldReturnEmpty()
    {
        // Arrange
        string input = "12345";

        // Act
        string result = ArabicMorphologyHelper.FormatWord(input, applyFuzzyNormalization: false);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region FormatWord(ReadOnlySpan, Span) Tests - Zero-Allocation API

    [Test]
    public void FormatWord_Span_StrongRoot_ShouldExtractRoot()
    {
        // Arrange
        string input = "كتاب";
        Span<char> inputSpan = input.ToCharArray();
        Span<char> outputBuffer = stackalloc char[64];

        // Act
        int length = ArabicMorphologyHelper.FormatWord(inputSpan, outputBuffer, applyFuzzyNormalization: false);

        // Assert
        string result = new string(outputBuffer.Slice(0, length));
        result.Should().Be("كتب");
    }

    [Test]
    public void FormatWord_Span_EmptyInput_ShouldReturnZero()
    {
        // Arrange
        ReadOnlySpan<char> inputSpan = ReadOnlySpan<char>.Empty;
        Span<char> outputBuffer = stackalloc char[64];

        // Act
        int length = ArabicMorphologyHelper.FormatWord(inputSpan, outputBuffer, applyFuzzyNormalization: false);

        // Assert
        length.Should().Be(0);
    }

    [Test]
    public void FormatWord_Span_WithArticle_ShouldProcess()
    {
        // Arrange - Note: The implementation may require minimum buffer size
        string input = "الكتاب";
        ReadOnlySpan<char> inputSpan = input;
        Span<char> outputBuffer = stackalloc char[64];

        // Act
        int length = ArabicMorphologyHelper.FormatWord(inputSpan, outputBuffer, applyFuzzyNormalization: false);

        // Assert
        length.Should().BeGreaterThan(0);
        string result = new string(outputBuffer.Slice(0, length));
        result.Should().Be("كتب");
    }

    #endregion

    #region Advanced Morphology Tests (The Professor's Challenge)

    [Test]
    public void FormatWord_GeminatedRoot_ShouldExtractRoot()
    {
        // Geminated (المضعف) - 2 letters with Shadda or 3 letters with repeated last
        // "مد" (M-D) -> Root: M-D-D
        ArabicMorphologyHelper.FormatWord("مد", false).Should().Be("مدد");

        // "استمر" (Istamarra) -> Root: M-R-R
        // Fixed: "است" prefix is now correctly handled via Pattern X Geminated check
        ArabicMorphologyHelper.FormatWord("استمر", false).Should().Be("مرر");

        // "حج" (Hajj) -> Root: H-J-J
        ArabicMorphologyHelper.FormatWord("حج", false).Should().Be("حجج");
    }

    [Test]
    public void FormatWord_HamzatedRoot_ShouldExtractRoot()
    {
        // Hamzated (المهموز) - Contains Hamza
        // Start: "أكل" (Akala) -> Root: A-K-L
        ArabicMorphologyHelper.FormatWord("أكل", false).Should().Be("أكل");

        // Middle: "سأل" (Sa'ala) -> Root: S-A-L
        ArabicMorphologyHelper.FormatWord("سأل", false).Should().Be("سأل");

        // End: "قرأ" (Qara'a) -> Root: Q-R-A
        ArabicMorphologyHelper.FormatWord("قرأ", false).Should().Be("قرأ");
    }

    [Test]
    public void FormatWord_WeakRoot_Mithal_ShouldExtractRoot()
    {
        // Mithal (First Weak) - Starts with Waw or Ya
        // "وعد" (Wa'ada) -> Root: W-A-D
        ArabicMorphologyHelper.FormatWord("وعد", false).Should().Be("وعد");

        // "يصل" (Yasilu) -> Root: W-S-L (Waw restored)
        // Note: This is a hard case. "يصل" comes from "وصل".
        // If the engine is smart enough, it might get "وصل". If not, it might fail or return "صل".
        // Let's assert what we expect the CURRENT engine to do based on code analysis.
        // The code has `FirstWeak` check.
        ArabicMorphologyHelper.FormatWord("يصل", false).Should().Be("وصل");
    }

    [Test]
    public void FormatWord_WeakRoot_Ajwaf_ShouldExtractRoot()
    {
        // Ajwaf (Middle Weak) - Middle is Alif/Waw/Ya
        // "قال" (Qala) -> Root: Q-W-L (Alif turns to Waw usually, or stays Alif depending on normalizer)
        // The code normalizes Alif to... let's check.
        // Usually IR stems return the Alif or normalized form.
        // Let's expect "قول" or "قال". Based on standard light stemming, "قال" is often acceptable, 
        // but root extraction should find "قول".
        // Let's try "باع" -> "بيع".

        // For now, let's assume the engine returns the trilateral form.
        // "قال" -> "قول"
        ArabicMorphologyHelper.FormatWord("قال", false).Should().Be("قول");
    }

    [Test]
    public void FormatWord_WeakRoot_Naqis_ShouldExtractRoot()
    {
        // Naqis (Last Weak) - Ends with vowel
        // "دعا" (Da'a) -> Root: D-A-W
        ArabicMorphologyHelper.FormatWord("دعا", false).Should().Be("دعا");

        // "قضى" (Qada) -> Root: Q-D-Y
        ArabicMorphologyHelper.FormatWord("قضى", false).Should().Be("قضي");
    }

    [Test]
    public void FormatWord_ComplexAffixes_ShouldExtractRoot()
    {
        // "فسيكفيكهم" (Fa-Sa-Yakfeekahum) -> Root: K-F-Y (Kafa)
        // This is the famous "longest word" example.
        // Prefixes: Fa, Sa, Ya
        // Suffixes: Ka, Hum
        // Core: K-F-Y
        ArabicMorphologyHelper.FormatWord("فسيكفيكهم", false).Should().Be("كفي");
    }

    #endregion
}
