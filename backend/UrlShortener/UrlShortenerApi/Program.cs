using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Data;
using UrlShortenerApi.Extensions;
using UrlShortenerApi.Models;
using UrlShortenerApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDatabaseContext(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("{code}", async (string code, ApplicationDbContext context) =>
{
    var shortenedUrl = await context.ShortenedUrls.FirstOrDefaultAsync(u => u.Code == code);

    if (shortenedUrl != null) return Results.Redirect(shortenedUrl.BaseUrl);
    else return Results.NotFound();
})
.WithOpenApi();

app.MapPost("/", async (string url, ApplicationDbContext context, UrlShorteningService urlShorteningService) =>
{
    var isUrlValid = Uri.TryCreate(url, UriKind.Absolute, out var uriResult) && 
        (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    if (!isUrlValid) return Results.BadRequest("Invalid URL");

    var code = await urlShorteningService.GenerateUniqueCode();

    var shortenedUrl = new ShortenedUrl()
    {
        Code = code,
        BaseUrl = url,
        ShortUrl = $"domain/{code}",
        CreatedOnUtc = DateTime.UtcNow
    };

    context.ShortenedUrls.Add(shortenedUrl);
    await context.SaveChangesAsync();

    return Results.Ok(shortenedUrl.ShortUrl);
})
.WithOpenApi();

app.Run();