using Microsoft.AspNetCore.Mvc;
using UrlShortenerApi.Extensions;
using UrlShortenerApi.Models;
using UrlShortenerApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDatabaseContext();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<UrlShorteningService>();
builder.Services.AddScoped<DatabaseService>();
builder.Services.AddScoped<CacheService>();
builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = Environment.GetEnvironmentVariable("RedisConnection");
    options.InstanceName = "s-url-";
});
builder.Services.AddCors();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod());

app.MapGet("{code}", async Task<IResult> (
    string code,
    DatabaseService databaseService,
    CacheService cacheService) =>
{
    var cachedUrl = await cacheService.GetCachedUrlAsync(code);
    if (cachedUrl != null) return Results.Redirect(cachedUrl.BaseUrl);

    var shortenedUrl = await databaseService.GetShortenedUrlByCodeAsync(code);
    if (shortenedUrl == null) return Results.NotFound();

    await cacheService.SetCacheAsync(shortenedUrl);

    return Results.Redirect(shortenedUrl.BaseUrl);
});

app.MapPost("/", async (
    [FromBody] ShortenedUrlRequest request,
    UrlShorteningService urlShorteningService,
    DatabaseService databaseService,
    CacheService cacheService) =>
{
    var isUrlValid = Uri.TryCreate(request.Url, UriKind.Absolute, out var uriResult) &&
        (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    if (!isUrlValid) return Results.BadRequest("Invalid URL");

    if (request.Url[request.Url.Length - 1] == '/') request.Url = request.Url.Remove(request.Url.Length - 1);

    var dbUrl = await databaseService.GetShortenedUrlByBaseUrlAsync(request.Url);
    if (dbUrl != null) return Results.Ok(dbUrl.ShortUrl);

    var shortenedUrl = await urlShorteningService.GenerateShortenedUrlAsync(request.Url);
    await databaseService.AddShortenedUrlAsync(shortenedUrl);
    await cacheService.SetCacheAsync(shortenedUrl);

    return Results.Ok(shortenedUrl.ShortUrl);
});

app.Run();