namespace Linguistics.Data;

/// <summary>
/// Contains compiled patterns for Arabic morphological analysis and diacritic normalization.
/// Patterns are optimized for high-performance matching.
/// </summary>
public static class ArabicPatternsData
{
    /// <summary>
    /// 
    /// </summary>
    public static readonly HashSet<string> MorphologyTemplates = new()
    {
        "فعّل" , "فاعل" , "افعل" , "تفعّل" , "تفعل" , "تفاعل" , "انفعل" , "افتعل" , "افعلّ" , "استفعل" ,
        "تفعيل" , "فعال" , "افعال" , "انفعال" , "افتعال" , "افعلال" , "استفعال" ,
        "مفعل" , "مفاعل" , "متفعّل" , "متفعل" , "متفاعل" , "منفعل" , "مفتعل" , "مفعلّ" , "مستفعل" ,
        "مفعول" ,
        "فعول" , "مفعال" , "فعّال" , "فعيل" ,
        "أفعل" , "فعلان" , "فعلاء" , "فعلى" ,
        "فواعل" , "مفاعيل" , "افاعل" , "فعيّل" ,
        "يفتعل" , "يستفعل" , "تفتعل" , "فعائل" ,
    };

    /// <summary>
    /// Provides a greedy-sorted array of normalization patterns for Arabic diacritics and orthographic marks. This
    /// array is used to standardize, remove, or replace complex and single diacritic combinations in Arabic text
    /// according to verified linguistic rules.
    /// </summary>
    /// <remarks>Patterns are ordered to ensure that complex diacritic combinations are matched and normalized
    /// before single marks, enabling correct and efficient normalization. The array includes mappings for both removal
    /// and replacement of diacritics, as well as handling of silent letters and orthographic markers. This collection
    /// is intended for use in text normalization routines where accurate processing of Arabic script is
    /// required.</remarks>
    public static readonly ArabicDiacriticsPattern[] DiacriticNormalization = new ArabicDiacriticsPattern[]
    {
        // ---------------------------------------------------------
        // 1. COMPLEX COMBINATIONS (Must come first for Greedy Match)
        // ---------------------------------------------------------

        new ArabicDiacriticsPattern("\u0621\u064E\u0627", ArabicConstants.AlefMedda, true, true), // Normalization: Hamza + Fatha + Alef -> Alef Madda
        new ArabicDiacriticsPattern("\u0635\u06DC", ArabicConstants.Seen, true, true), // Normalization: Sad + High Seen -> Seen
        new ArabicDiacriticsPattern("\u0648\u0670", ArabicConstants.Alef, true, true), // Normalization: Waw + Superscript Alef -> Alef

        new ArabicDiacriticsPattern("\u0670\u0653", "", true, false), // Superscript Alef + Madda

        // Shadda Combinations (Remove these before removing Shadda alone)
        new ArabicDiacriticsPattern("\u0651\u064E", "", false, true), // Shadda + Fatha
        new ArabicDiacriticsPattern("\u0651\u064F", "", false, true), // Shadda + Damma
        new ArabicDiacriticsPattern("\u0651\u0650", "", false, true), // Shadda + Kasra
        new ArabicDiacriticsPattern("\u0651\u064C", "", false, true), // Shadda + Dammatan
        new ArabicDiacriticsPattern("\u0651\u064B", "", false, true), // Shadda + Fathatan
        new ArabicDiacriticsPattern("\u0651\u064D", "", false, true), // Shadda + Kasratan

        // Silent Letters / Orthographic Markers
        new ArabicDiacriticsPattern("\u0627\u0652", "", true, false), // Alef + Sukun
        new ArabicDiacriticsPattern("\u0648\u0652", "", true, false), // Waw + Sukun
        new ArabicDiacriticsPattern("\u064A\u0652", "", true, false), // Yeh + Sukun
        new ArabicDiacriticsPattern("\u0627\u06DF", "", true, false), // Alef + Rounded Zero
        new ArabicDiacriticsPattern("\u0648\u06DF", "", true, false), // Waw + Rounded Zero
        new ArabicDiacriticsPattern("\u064A\u06DF", "", true, false), // Yeh + Rounded Zero

        // ---------------------------------------------------------
        // 2. SINGLE DIACRITICS & MARKS
        // ---------------------------------------------------------

        new ArabicDiacriticsPattern("\u064B", "", false, true), // Fathatan
        new ArabicDiacriticsPattern("\u064C", "", false, true), // Dammatan
        new ArabicDiacriticsPattern("\u064D", "", false, true), // Kasratan
        new ArabicDiacriticsPattern("\u064E", "", false, true), // Fatha
        new ArabicDiacriticsPattern("\u064F", "", false, true), // Damma
        new ArabicDiacriticsPattern("\u0650", "", false, true), // Kasra
        new ArabicDiacriticsPattern("\u0651", "", false, true), // Shadda
        new ArabicDiacriticsPattern("\u0652", "", false, true), // Sukun
        new ArabicDiacriticsPattern("\u0640", "", true, true),  // Tatweel

        new ArabicDiacriticsPattern("\u08F0", "", true, true), // Open Fathatan
        new ArabicDiacriticsPattern("\u08F1", "", true, true), // Open Dammatan
        new ArabicDiacriticsPattern("\u08F2", "", true, true), // Open Kasratan

        new ArabicDiacriticsPattern("\u065E", "", true, true), // Fatha with Two Dots
        new ArabicDiacriticsPattern("\u0656", "", true, true), // Subscript Alef
        new ArabicDiacriticsPattern("\u0657", "", true, true), // Inverted Damma
        new ArabicDiacriticsPattern("\u0658", "", true, true), // Mark Noon Ghunna
        new ArabicDiacriticsPattern("\u0659", "", true, true), // Zwarakay
        new ArabicDiacriticsPattern("\u065A", "", true, true), // Vowel Sign Small V Above

        new ArabicDiacriticsPattern("\u0653", "", true, true), // Maddah Above
        new ArabicDiacriticsPattern("\u0654", "", true, true), // Hamza Above (Combining) - Mapped to empty to preserve carrier
        new ArabicDiacriticsPattern("\u0655", "", true, true), // Hamza Below (Combining)

        new ArabicDiacriticsPattern("\u0670", ArabicConstants.Alef, true, true), // Superscript Alef
        new ArabicDiacriticsPattern("\u0671", ArabicConstants.Alef, true, true), // Alef Wasla

        new ArabicDiacriticsPattern("\u06E1", "", true, true), // Small High Dotless Head of Khah
        new ArabicDiacriticsPattern("\u06E4", "", true, true), // Small High Madda
        new ArabicDiacriticsPattern("\u06E5", ArabicConstants.Waw, true, true), // Small Waw
        new ArabicDiacriticsPattern("\u08F3", ArabicConstants.Waw, true, true), // Small High Waw
        new ArabicDiacriticsPattern("\u06E6", ArabicConstants.Ya, true, true), // Small Yeh
        new ArabicDiacriticsPattern("\u06E7", ArabicConstants.Ya, true, true), // Small High Yeh

        new ArabicDiacriticsPattern("\u0615", "", true, true), // Small High Tah
        new ArabicDiacriticsPattern("\uFBC0", "", true, true), // Small Tah Above
        new ArabicDiacriticsPattern("\uFBC1", "", true, true), // Small Tah Below
        new ArabicDiacriticsPattern("\u0617", "", true, true), // Small High Zain
        new ArabicDiacriticsPattern("\u06DA", "", true, true), // Small High Jeem
        new ArabicDiacriticsPattern("\u06DC", "", true, true), // Small Low Seen
        new ArabicDiacriticsPattern("\u06E3", "", true, true), // Small High Seen
        new ArabicDiacriticsPattern("\u06E8", "", true, true), // Small High Noon
        new ArabicDiacriticsPattern("\u06D8", "", true, true), // Small High Meem Initial
        new ArabicDiacriticsPattern("\u06ED", "", true, true), // Small Low Meem
        new ArabicDiacriticsPattern("\u06E2", "", true, true), // Small High Meem Isolated
        new ArabicDiacriticsPattern("\u0616", "", true, true), // Small High Ligature Alef-Lam-Yeh
        new ArabicDiacriticsPattern("\u06D6", "", true, true), // Small High Ligature Sad-Lam-Alef-Maksura
        new ArabicDiacriticsPattern("\u06D7", "", true, true), // Small High Ligature Qaf-Lam-Alef-Maksura
        new ArabicDiacriticsPattern("\u06D9", "", true, true), // Small High Lam Alef
        new ArabicDiacriticsPattern("\u06DB", "", true, true), // Small High Three Dots

        new ArabicDiacriticsPattern("\u06E0", "", true, true), // Small High Upright Rectangular Zero
        new ArabicDiacriticsPattern("\u06DF", "", true, true), // Small High Rounded Zero

        new ArabicDiacriticsPattern("\u06DD", "", true, true), // End of Ayah
        new ArabicDiacriticsPattern("\u06DE", "", true, true), // Start of Rub El Hizb
        new ArabicDiacriticsPattern("\u06E9", "", true, true), // Place of Sajdah
        new ArabicDiacriticsPattern("\u06EA", "", true, true), // Empty Centre Low Stop
        new ArabicDiacriticsPattern("\u06EB", "", true, true), // Empty Centre High Stop
        new ArabicDiacriticsPattern("\u06EC", "", true, true), // Rounded High Stop
    };
}
