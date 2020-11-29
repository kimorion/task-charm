namespace Charm.Core.Domain.Utils
{
    public static class SpeechDictionary
    {
        public static readonly string[] TodayGistListVariants = new[]
        {
            "сегодня",
            "сегодн",
            "сейчас",
            "сейч",
        };

        public static readonly string[] TomorrowGistListVariants = new[]
        {
            "завтра",
            "завтр",
            "завт",
            "на завтра",
        };
        
        public static readonly string[] YesterdayGistListVariants = new[]
        {
            "вчера",
            "за вчера",
            "вчерашние",
        };
    }
}