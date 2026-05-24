namespace Linguistics.Data;

/// <summary>
/// Defines constant string representations for Arabic characters and diacritics.
/// Centralizes character definitions to avoid magic strings.
/// </summary>
public static class ArabicConstants
{
    // --- Core Arabic Letters ---
    public const string Alef = "\u0627";          // "ا" Alif (basic form)

    public const string AlefMedda = "\u0622";     // "آ" Alif with Maddah (elongation mark, Quranic usage)
    public const string AlefHamzaAbove = "\u0623"; // "أ" Alif with Hamza above
    public const string AlefHamzaBelow = "\u0625"; // "إ" Alif with Hamza below
    public const string AlefMaqsura = "\u0649";   // "ى" Alif Maqsura (looks like Ya without dots, Quranic usage)

    public const string Ba = "\u0628";            // "ب" Ba
    public const string Ta = "\u062A";            // "ت" Ta

    public const string TaaMarbuta = "\u0629";    // "ة" Taa Marbuta (used at word endings, feminine marker)

    public const string Tha = "\u062B";           // "ث" Tha (Th sound)

    public const string Jeem = "\u062C";          // "ج" Jeem
    public const string Hha = "\u062D";           // "ح" Hha (deep H sound)
    public const string Kha = "\u062E";           // "خ" Kha

    public const string Dal = "\u062F";           // "د" Dal
    public const string Thal = "\u0630";          // "ذ" Thal (Dh sound)

    public const string Ra = "\u0631";            // "ر" Ra
    public const string Zay = "\u0632";           // "ز" Zay

    public const string Seen = "\u0633";          // "س" Seen
    public const string Sheen = "\u0634";         // "ش" Sheen

    public const string Sad = "\u0635";           // "ص" Sad (emphatic S)
    public const string Dad = "\u0636";           // "ض" Dad (emphatic D, unique to Arabic)

    public const string Tta = "\u0637";           // "ط" Tta (emphatic T)
    public const string Dha = "\u0638";           // "ظ" Dha (emphatic Dh)

    public const string Ain = "\u0639";           // "ع" Ain (deep voiced sound)
    public const string Ghain = "\u063A";         // "غ" Ghain

    public const string Fa = "\u0641";            // "ف" Fa
    public const string Qaf = "\u0642";           // "ق" Qaf

    public const string Kaf = "\u0643";           // "ك" Kaf
    public const string Lam = "\u0644";           // "ل" Lam
    public const string Meem = "\u0645";          // "م" Meem
    public const string Noon = "\u0646";          // "ن" Noon

    public const string Ha = "\u0647";            // "ه" Ha
    public const string Waw = "\u0648";           // "و" Waw
    public const string Ya = "\u064A";            // "ي" Ya

    // --- Quranic & Special Forms ---
    public const string Hamza = "\u0621";         // "ء" Standalone Hamza

    public const string HamzaOnWaw = "\u0624";    // "ؤ" Hamza on Waw
    public const string HamzaOnYa = "\u0626";     // "ئ" Hamza on Ya (different from Alef Maqsura)

    // --- Additional Quranic Orthography ---
    public const string SmallAlef = "\u0670";        // "ٰ" Small Alif (dagger Alif, appears in Quranic text above letters)

    public const string SuperscriptAlef = "\u0671";  // "ٱ" Alif Wasla (used in Quranic orthography, e.g., ٱللَّه)
    public const string Tatweel = "\u0640";          // "ـ" Tatweel (kashida, elongation mark for calligraphy/Quranic text)

    // --- Quranic Marks ---
    public const string VerseSeparator = "\u06DD";   // "۝" Quranic verse separator

    public const string EndOfAyah = "\u06DD";        // "۝" End of Ayah mark
    public const string StartOfRubElHizb = "\u06DE"; // "۞" Rub El Hizb (section marker)
    public const string SmallHighMeem = "\u06DE";    // "۞" Stop mark in Quranic text

    // --- Quranic Annotation Signs ---
    public const string SmallHighLigatureSadLamAlef = "\u06D6"; // "ۖ" Small high ligature Sad-Lam-Alef

    public const string SmallLowMeem = "\u06ED";                // "ۭ" Small low Meem

    // --- Extended Quranic Marks ---
    public const string OpenMarkAbove = "\u08F0";  // "ࣰ" Quranic annotation mark above

    public const string OpenMarkBelow = "\u08F3";  // "ࣳ" Quranic annotation mark below

    // --- Additional Combining Marks ---
    public const string SmallHighTah = "\u0615";   // "ؕ" Small high Tah

    public const string SmallHighSeen = "\u0617";  // "ؗ" Small high Seen

    // --- Arabic Presentation Forms ---
    public const string RumiDigitOne = "\uFBC0";   // "﯀" Rumi digit one

    public const string RumiDigitTwo = "\uFBC1";   // "﯁" Rumi digit two

    // --- Diacritics (Tashkeel) ---
    public const string Fatha = "\u064E";         // "َ" Short vowel: a

    public const string Damma = "\u064F";         // "ُ" Short vowel: u
    public const string Kasra = "\u0650";         // "ِ" Short vowel: i
    public const string Sukoon = "\u0652";        // "ْ" No vowel
    public const string Shadda = "\u0651";        // "ّ" Consonant doubling
    public const string TanwinFath = "\u064B";    // "ً" Tanwin (double Fatha)
    public const string TanwinDamm = "\u064C";    // "ٌ" Tanwin (double Damma)
    public const string TanwinKasr = "\u064D";    // "ٍ" Tanwin (double Kasra)

    // --- Advanced Combining Marks (U+0653–U+065F) ---
    public const string MaddahAbove = "\u0653";        // "ٓ" Maddah above (elongation mark)

    public const string HamzaAbove = "\u0654";         // "ٔ" Hamza above (combining mark)
    public const string HamzaBelow = "\u0655";         // "ٕ" Hamza below (combining mark)
    public const string SubscriptAlef = "\u0656";      // "ٖ" Subscript Alef (Quranic usage)
    public const string InvertedDamma = "\u0657";      // "ٗ" Inverted Damma
    public const string MarkNoVowel = "\u0658";        // "٘" Mark no vowel (Quranic annotation)
    public const string ZigzagFatha = "\u0659";        // "ٙ" Zigzag Fatha
    public const string DotAbove = "\u065A";           // "ٚ" Dot above
    public const string ReversedDamma = "\u065B";      // "ٛ" Reversed Damma
    public const string FathaWithTwoDots = "\u065C";   // "ٜ" Fatha with two dots
    public const string WavyHamzaAbove = "\u065D";     // "ٝ" Wavy Hamza above
    public const string WavyHamzaBelow = "\u065E";     // "ٞ" Wavy Hamza below
    public const string VerticalZigzagFatha = "\u065F";// "ٟ" Vertical zigzag Fatha

    // --- Arabic Numerals ---
    public const string Zero = "\u0660";   // "٠" Arabic-Indic digit zero

    public const string One = "\u0661";    // "١" Arabic-Indic digit one
    public const string Two = "\u0662";    // "٢" Arabic-Indic digit two
    public const string Three = "\u0663";  // "٣" Arabic-Indic digit three
    public const string Four = "\u0664";   // "٤" Arabic-Indic digit four
    public const string Five = "\u0665";   // "٥" Arabic-Indic digit five
    public const string Six = "\u0666";    // "٦" Arabic-Indic digit six
    public const string Seven = "\u0667";  // "٧" Arabic-Indic digit seven
    public const string Eight = "\u0668";  // "٨" Arabic-Indic digit eight
    public const string Nine = "\u0669";   // "٩" Arabic-Indic digit nine

    // --- Common Ligatures ---
    public const string LigatureAllah = "\uFDF2";       // "ﷲ" Allah ligature

    public const string LigatureLamAlef = "\uFEFB";     // "ﻻ" Lam + Alef isolated
    public const string LigatureLamAlefFinal = "\uFEFC"; // "ﻼ" Lam + Alef final form

    // --- Arabic Presentation Forms (Lam-Alef Variants) ---
    public const string LamAlefIsolated = "\uFEFB";     // "ﻻ" Lam-Alef isolated form

    public const string LamAlefFinal = "\uFEFC";        // "ﻼ" Lam-Alef final form
    public const string LamAlefWithHamzaAboveIsolated = "\uFEF7"; // "ﻷ" Lam-Alef with Hamza above isolated
    public const string LamAlefWithHamzaAboveFinal = "\uFEF8";    // "ﻸ" Lam-Alef with Hamza above final
    public const string LamAlefWithHamzaBelowIsolated = "\uFEF9"; // "ﻹ" Lam-Alef with Hamza below isolated
    public const string LamAlefWithHamzaBelowFinal = "\uFEFA";    // "ﻺ" Lam-Alef with Hamza below final

    public const string CommonDiacritics =
        TanwinFath + // ً  Fathatan
        TanwinDamm + // ٌ  Dammatan
        TanwinKasr + // ٍ  Kasratan
        Fatha + // َ  Fatha
        Damma + // ُ  Damma
        Kasra + // ِ  Kasra
        Shadda + // ّ  Shadda
        Sukoon;  // ْ  Sukun

    public const string QuranicMarks =
        MaddahAbove + // ٓ  Maddah above
        HamzaAbove + // ٔ  Hamza above
        HamzaBelow + // ٕ  Hamza below
        SubscriptAlef + // ٖ  Subscript Alef
        InvertedDamma + // ٗ  Inverted Damma
        MarkNoVowel + // ٘  Mark Noon Ghunna
        ZigzagFatha + // ٙ  Zigzag above
        DotAbove + // ٚ  Small V above
        ReversedDamma;  // ٛ  V above

    public const string RareMarks =
        FathaWithTwoDots + // ٜ  Small Waw
        WavyHamzaAbove + // ٝ  Small high rounded zero
        WavyHamzaBelow + // ٞ  Three dots below
        VerticalZigzagFatha;  // ٟ  Wavy Hamza Below
}

public static class ArabicConstantsChar
{
    public const char Null = '\u0000';

    // --- Core Arabic Letters ---
    public const char Alef = '\u0627';          // "ا" Alif (basic form)

    public const char AlefMedda = '\u0622';     // "آ" Alif with Maddah (elongation mark, Quranic usage)
    public const char AlefHamzaAbove = '\u0623'; // "أ" Alif with Hamza above
    public const char AlefHamzaBelow = '\u0625'; // "إ" Alif with Hamza below
    public const char AlefMaqsura = '\u0649';   // "ى" Alif Maqsura (looks like Ya without dots, Quranic usage)

    public const char Ba = '\u0628';            // "ب" Ba
    public const char Ta = '\u062A';            // "ت" Ta

    public const char TaaMarbuta = '\u0629';    // "ة" Taa Marbuta (used at word endings, feminine marker)

    public const char Tha = '\u062B';           // "ث" Tha (Th sound)

    public const char Jeem = '\u062C';          // "ج" Jeem
    public const char Hha = '\u062D';           // "ح" Hha (deep H sound)
    public const char Kha = '\u062E';           // "خ" Kha

    public const char Dal = '\u062F';           // "د" Dal
    public const char Thal = '\u0630';          // "ذ" Thal (Dh sound)

    public const char Ra = '\u0631';            // "ر" Ra
    public const char Zay = '\u0632';           // "ز" Zay

    public const char Seen = '\u0633';          // "س" Seen
    public const char Sheen = '\u0634';         // "ش" Sheen

    public const char Sad = '\u0635';           // "ص" Sad (emphatic S)
    public const char Dad = '\u0636';           // "ض" Dad (emphatic D, unique to Arabic)

    public const char Tta = '\u0637';           // "ط" Tta (emphatic T)
    public const char Dha = '\u0638';           // "ظ" Dha (emphatic Dh)

    public const char Ain = '\u0639';           // "ع" Ain (deep voiced sound)
    public const char Ghain = '\u063A';         // "غ" Ghain

    public const char Fa = '\u0641';            // "ف" Fa
    public const char Qaf = '\u0642';           // "ق" Qaf

    public const char Kaf = '\u0643';           // "ك" Kaf
    public const char Lam = '\u0644';           // "ل" Lam
    public const char Meem = '\u0645';          // "م" Meem
    public const char Noon = '\u0646';          // "ن" Noon

    public const char Ha = '\u0647';            // "ه" Ha
    public const char Waw = '\u0648';           // "و" Waw
    public const char Ya = '\u064A';            // "ي" Ya

    // --- Quranic & Special Forms ---
    public const char Hamza = '\u0621';         // "ء" Standalone Hamza

    public const char HamzaOnWaw = '\u0624';    // "ؤ" Hamza on Waw
    public const char HamzaOnYa = '\u0626';     // "ئ" Hamza on Ya (different from Alef Maqsura)

    // --- Additional Quranic Orthography ---
    public const char SmallAlef = '\u0670';        // "ٰ" Small Alif (dagger Alif, appears in Quranic text above letters)

    public const char SuperscriptAlef = '\u0671';  // "ٱ" Alif Wasla (used in Quranic orthography, e.g., ٱللَّه)
    public const char Tatweel = '\u0640';          // "ـ" Tatweel (kashida, elongation mark for calligraphy/Quranic text)

    // --- Quranic Marks ---
    public const char VerseSeparator = '\u06DD';   // "۝" Quranic verse separator

    public const char EndOfAyah = '\u06DD';        // "۝" End of Ayah mark
    public const char StartOfRubElHizb = '\u06DE'; // "۞" Rub El Hizb (section marker)
    public const char SmallHighMeem = '\u06DE';    // "۞" Stop mark in Quranic text

    // --- Quranic Annotation Signs ---
    public const char SmallHighLigatureSadLamAlef = '\u06D6'; // "ۖ" Small high ligature Sad-Lam-Alef

    public const char SmallLowMeem = '\u06ED';                // "ۭ" Small low Meem

    // --- Extended Quranic Marks ---
    public const char OpenMarkAbove = '\u08F0';  // "ࣰ" Quranic annotation mark above

    public const char OpenMarkBelow = '\u08F3';  // "ࣳ" Quranic annotation mark below

    // --- Additional Combining Marks ---
    public const char SmallHighTah = '\u0615';   // "ؕ" Small high Tah

    public const char SmallHighSeen = '\u0617';  // "ؗ" Small high Seen

    // --- Arabic Presentation Forms ---
    public const char RumiDigitOne = '\uFBC0';   // "﯀" Rumi digit one

    public const char RumiDigitTwo = '\uFBC1';   // "﯁" Rumi digit two

    // --- Diacritics (Tashkeel) ---
    public const char Fatha = '\u064E';         // "َ" Short vowel: a

    public const char Damma = '\u064F';         // "ُ" Short vowel: u
    public const char Kasra = '\u0650';         // "ِ" Short vowel: i
    public const char Sukoon = '\u0652';        // "ْ" No vowel
    public const char Shadda = '\u0651';        // "ّ" Consonant doubling
    public const char TanwinFath = '\u064B';    // "ً" Tanwin (double Fatha)
    public const char TanwinDamm = '\u064C';    // "ٌ" Tanwin (double Damma)
    public const char TanwinKasr = '\u064D';    // "ٍ" Tanwin (double Kasra)

    // --- Advanced Combining Marks (U+0653–U+065F) ---
    public const char MaddahAbove = '\u0653';        // "ٓ" Maddah above (elongation mark)

    public const char HamzaAbove = '\u0654';         // "ٔ" Hamza above (combining mark)
    public const char HamzaBelow = '\u0655';         // "ٕ" Hamza below (combining mark)
    public const char SubscriptAlef = '\u0656';      // "ٖ" Subscript Alef (Quranic usage)
    public const char InvertedDamma = '\u0657';      // "ٗ" Inverted Damma
    public const char MarkNoVowel = '\u0658';        // "٘" Mark no vowel (Quranic annotation)
    public const char ZigzagFatha = '\u0659';        // "ٙ" Zigzag Fatha
    public const char DotAbove = '\u065A';           // "ٚ" Dot above
    public const char ReversedDamma = '\u065B';      // "ٛ" Reversed Damma
    public const char FathaWithTwoDots = '\u065C';   // "ٜ" Fatha with two dots
    public const char WavyHamzaAbove = '\u065D';     // "ٝ" Wavy Hamza above
    public const char WavyHamzaBelow = '\u065E';     // "ٞ" Wavy Hamza below
    public const char VerticalZigzagFatha = '\u065F';// "ٟ" Vertical zigzag Fatha

    // --- Arabic Numerals ---
    public const char Zero = '\u0660';   // "٠" Arabic-Indic digit zero

    public const char One = '\u0661';    // "١" Arabic-Indic digit one
    public const char Two = '\u0662';    // "٢" Arabic-Indic digit two
    public const char Three = '\u0663';  // "٣" Arabic-Indic digit three
    public const char Four = '\u0664';   // "٤" Arabic-Indic digit four
    public const char Five = '\u0665';   // "٥" Arabic-Indic digit five
    public const char Six = '\u0666';    // "٦" Arabic-Indic digit six
    public const char Seven = '\u0667';  // "٧" Arabic-Indic digit seven
    public const char Eight = '\u0668';  // "٨" Arabic-Indic digit eight
    public const char Nine = '\u0669';   // "٩" Arabic-Indic digit nine

    // --- Common Ligatures ---
    public const char LigatureAllah = '\uFDF2';       // "ﷲ" Allah ligature

    public const char LigatureLamAlef = '\uFEFB';     // "ﻻ" Lam + Alef isolated
    public const char LigatureLamAlefFinal = '\uFEFC'; // "ﻼ" Lam + Alef final form

    // --- Arabic Presentation Forms (Lam-Alef Variants) ---
    public const char LamAlefIsolated = '\uFEFB';     // "ﻻ" Lam-Alef isolated form

    public const char LamAlefFinal = '\uFEFC';        // "ﻼ" Lam-Alef final form
    public const char LamAlefWithHamzaAboveIsolated = '\uFEF7'; // "ﻷ" Lam-Alef with Hamza above isolated
    public const char LamAlefWithHamzaAboveFinal = '\uFEF8';    // "ﻸ" Lam-Alef with Hamza above final
    public const char LamAlefWithHamzaBelowIsolated = '\uFEF9'; // "ﻹ" Lam-Alef with Hamza below isolated
    public const char LamAlefWithHamzaBelowFinal = '\uFEFA';    // "ﻺ" Lam-Alef with Hamza below final
}
