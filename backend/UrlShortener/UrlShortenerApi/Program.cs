using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using UrlShortenerApi.Data;
using UrlShortenerApi.Extensions;
using UrlShortenerApi.Models;
using UrlShortenerApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDatabaseContext(builder.Configuration);
builder.Services.AddScoped<UrlShorteningService>();
builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = "localhost";
    options.InstanceName = "local";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("{code}", async Task<IResult> (string code, ApplicationDbContext context, IDistributedCache cache) =>
{
    var cachedUrl = await cache.GetStringAsync(code);
    if (cachedUrl != null)
    {
        var deserializedUrl = JsonSerializer.Deserialize<ShortenedUrl>(cachedUrl)!;
        return Results.Redirect(deserializedUrl.BaseUrl);
    }

    var shortenedUrl = await context.ShortenedUrls.FirstOrDefaultAsync(u => u.Code == code);
    if (shortenedUrl == null) return Results.NotFound();

    var serialized = JsonSerializer.Serialize(shortenedUrl);
    await cache.SetStringAsync(shortenedUrl.Code, serialized, new DistributedCacheEntryOptions()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    });

    return Results.Redirect(shortenedUrl.BaseUrl);
})
.WithOpenApi();

app.MapPost("/", async (
    [FromBody] ShortenedUrlRequest request, 
    ApplicationDbContext dbContext, 
    UrlShorteningService urlShorteningService,
    HttpContext httpContext,
    IDistributedCache cache) =>
{
    var isUrlValid = Uri.TryCreate(request.Url, UriKind.Absolute, out var uriResult) && 
        (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    if (!isUrlValid) return Results.BadRequest("Invalid URL");

    var code = await urlShorteningService.GenerateUniqueCode();

    var shortenedUrl = new ShortenedUrl()
    {
        Code = code,
        BaseUrl = request.Url,
        ShortUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{code}",
        CreatedOnUtc = DateTime.UtcNow
    };

    dbContext.ShortenedUrls.Add(shortenedUrl);
    await dbContext.SaveChangesAsync();

    var serialized = JsonSerializer.Serialize(shortenedUrl);
    await cache.SetStringAsync(shortenedUrl.Code, serialized, new DistributedCacheEntryOptions()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    });

    return Results.Ok(shortenedUrl.ShortUrl);
})
.WithOpenApi();

app.Run();