using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Data;
using UrlShortenerApi.Models;

namespace UrlShortenerApi.Services
{
    public class DatabaseService
    {
        private readonly ApplicationDbContext _context;

        public DatabaseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddShortenedUrlAsync(ShortenedUrl shortenedUrl)
        {
            _context.ShortenedUrls.Add(shortenedUrl);
            await _context.SaveChangesAsync();
        }

        public async Task<ShortenedUrl?> GetShortenedUrlByCodeAsync(string code)
        {
            return await _context.ShortenedUrls.FirstOrDefaultAsync(u => u.Code == code);
        }

        public async Task<ShortenedUrl?> GetShortenedUrlByBaseUrlAsync(string url)
        {
            return await _context.ShortenedUrls.FirstOrDefaultAsync(u => u.BaseUrl == url);
        }
    }
}