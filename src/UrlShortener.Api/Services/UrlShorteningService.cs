using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Configurations;
using UrlShortenerApi.Data;
using UrlShortenerApi.Models;

namespace UrlShortenerApi.Services
{
    public class UrlShorteningService
    {
        private readonly Random _random;
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UrlShorteningService(ApplicationDbContext context, IHttpContextAccessor contextAccessor)
        {
            _random = new Random();
            _dbContext = context;
            _httpContextAccessor = contextAccessor;
        }

        public async Task<ShortenedUrl> GenerateShortenedUrlAsync(string url)
        {         
            var code = await GenerateUniqueCode();

            var shortenedUrl = new ShortenedUrl()
            {
                Code = code,
                BaseUrl = url,
                ShortUrl = $"{_httpContextAccessor.HttpContext!.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/{code}",
                CreatedOnUtc = DateTime.UtcNow
            };

            return shortenedUrl;
        }

        private async Task<string> GenerateUniqueCode()
        {
            while (true)
            {
                var code = GenerateCode();
                var codeExists = await _dbContext.ShortenedUrls.AnyAsync(u => u.Code == code);
                if (!codeExists) return code;
            }
        }

        private string GenerateCode()
        {
            var code = new char[UrlShortenerConfiguration.CodeLength];
            
            for (int i = 0; i < code.Length; i++)
            {
                var currentCharIndex = _random.Next(0, UrlShortenerConfiguration.AlphabetSize);
                code[i] = UrlShortenerConfiguration.Alphabet[currentCharIndex];
            }

            return new string(code);
        }
    }
}