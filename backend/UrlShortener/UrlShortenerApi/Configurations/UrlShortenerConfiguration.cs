namespace UrlShortenerApi.Configurations
{
    public static class UrlShortenerConfiguration
    {
        public const int CodeLength = 7;
        public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        public static int AlphabetSize => Alphabet.Length;
    }
}