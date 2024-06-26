using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Data;

namespace UrlShortenerApi.Extensions
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddDbContext<ApplicationDbContext>(options => options
                .UseNpgsql(configuration.GetConnectionString("PostgreSqlConnection"))
                .UseSnakeCaseNamingConvention());
        }
    }
}