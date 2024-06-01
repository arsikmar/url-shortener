using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Data;
using UrlShortenerApi.Extensions;
using UrlShortenerApi.Models;
using UrlShortenerApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDatabaseContext(builder.Configuration);
builder.Services.AddScoped<UrlShorteningService>();

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

app.MapPost("/", async (
    [FromBody] ShortenedUrlRequest request, 
    ApplicationDbContext dbContext, 
    UrlShorteningService urlShorteningService,
    HttpContext httpContext) =>
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

    return Results.Ok(shortenedUrl.ShortUrl);
})
.WithOpenApi();

app.Run();