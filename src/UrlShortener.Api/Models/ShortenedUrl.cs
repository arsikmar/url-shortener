namespace UrlShortenerApi.Models
{
    public class ShortenedUrl
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string ShortUrl { get; set; } = string.Empty;
        public DateTime CreatedOnUtc { get; set; }
    }
}