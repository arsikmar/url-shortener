using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Data;

namespace UrlShortenerApi.Extensions
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddDatabaseContext(this IServiceCollection services)
        {
            return services.AddDbContext<ApplicationDbContext>(options => options
                .UseNpgsql(Environment.GetEnvironmentVariable("PostgreSqlConnection"))
                .UseSnakeCaseNamingConvention());
        }
    }
}