namespace LLG
{
    public class LanguageInfo
    {
        public readonly string Code;        // folder name under Resources/Languages
        public readonly string NativeName;
        public readonly string EnglishName;
        public readonly string Tagline;     // shown on the title screen
        public readonly bool Available;     // false = "coming soon"

        public LanguageInfo(string code, string nativeName, string englishName, string tagline, bool available)
        {
            Code = code;
            NativeName = nativeName;
            EnglishName = englishName;
            Tagline = tagline;
            Available = available;
        }
    }

    /// <summary>One place for values the whole game shares. Change the title or currency name here.</summary>
    public static class GameConfig
    {
        public const string Title = "RUNES OF SPEECH";
        public const string CurrencyName = "Verba";          // the magical currency
        public const int StartingCurrency = 60;
        public const string EnglishTagline = "A language-learning adventure";

        public static readonly LanguageInfo[] Languages =
        {
            new LanguageInfo("es", "Español", "Spanish", "Una aventura para aprender idiomas", true),
            new LanguageInfo("ja", "日本語", "Japanese", "ことばを学ぶ冒険", false),
            new LanguageInfo("ko", "한국어", "Korean", "언어를 배우는 모험", false),
        };
    }
}
