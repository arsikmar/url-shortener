using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Configurations;
using UrlShortenerApi.Data;

namespace UrlShortenerApi.Services
{
    public class UrlShorteningService
    {
        private readonly Random _random;
        private readonly ApplicationDbContext _context;

        public UrlShorteningService(ApplicationDbContext context)
        {
            _context = context;
            _random = new Random();
        }

        public async Task<string> GenerateUniqueCode()
        {
            while (true)
            {
                var code = GenerateCode();
                var codeExists = await _context.ShortenedUrls.AnyAsync(u => u.Code == code);
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