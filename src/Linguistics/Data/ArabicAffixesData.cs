namespace Linguistics.Data;

/// <summary>
/// Contains static data for Arabic affixes (prefixes, suffixes, definite articles).
/// Used for morphological analysis and stemming.
/// </summary>
public static class ArabicAffixesData
{
    public static readonly HashSet<string> DefiniteArticles = new()
    {
        "ال", "وال", "بال", "كال", "فال"
    };

    public static readonly HashSet<string> Prefixes = new()
    {
        "ال", "لل", "ل", "ا", "و", "س", "ب", "ي", "ن", "م", "ت", "ف", "است"
    };

    public static readonly HashSet<string> Suffixes = new()
    {
        "هما", "تما", "كما", "ان", "ها", "وا", "تم", "كم", "تن", "كن", "نا", "تا", "ما", "ون", "ين", "هن", "هم", "ته", "تي", "ني", "ن", "ك", "ه", "ة", "ت", "ا", "ي", "ات"
    };
}
