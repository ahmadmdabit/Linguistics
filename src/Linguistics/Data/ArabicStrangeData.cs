namespace Linguistics.Data;

/// <summary>
/// Contains a list of "strange" or foreign words that should be excluded from root extraction.
/// Includes arabized terms and common non-Arabic names.
/// </summary>
public static class ArabicStrangeData
{
    public static readonly HashSet<string> All = new()
    {
        "خوجة" , "بلورة" , "وكوفي" , "كوفي" , "تلفزة" , "ديسمبر" , "مانديلا" , "فرنسا" , "شيراك" , "إيران" , "نيلسون" , "الفرنسي" , "متلفزة" , "الأوروبية" , "تحتلها" ,
    };
}
