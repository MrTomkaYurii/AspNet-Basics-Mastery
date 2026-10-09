using System.Threading.RateLimiting;
using Common;
using Microsoft.AspNetCore.RateLimiting;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 24. Обмеження частоти запитів (Rate Limiting)
//
// Вбудована підсистема ASP.NET Core для захисту від DDoS, brute-force та
// перевищення лімітів навантаження на систему (RFC 6585 — 429 Too Many Requests).
//
// Демонструємо алгоритми:
//   1. Fixed Window (фіксоване вікно) — просте вікно часу;
//   2. Sliding Window (ковзне вікно) — більш плавний розподіл запитів;
//   3. Token Bucket (кошик маркерів) — дозволяє сплески (bursts) з подальшим стабільним лімітом;
//   4. Обробник відхилення OnRejected з заголовком Retry-After та форматом ProblemDetails.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ
// ======================================================================
builder.Services.AddCatalog();

builder.Services.AddRateLimiter(options =>
{
    // Глобальний обробник відхилених запитів (коли ліміт вичерпано)
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
        }

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc6585#section-4",
            title = "Забагато запитів (Too Many Requests)",
            status = StatusCodes.Status429TooManyRequests,
            detail = "Ви перевищили встановлений ліміт запитів. Спробуйте пізніше.",
            retryAfterSeconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var ra) ? (int)ra.TotalSeconds : 0
        }, cancellationToken);
    };

    // 1. Політика Fixed Window: максимум 3 запити за кожні 10 секунд
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 3;
        opt.Window = TimeSpan.FromSeconds(10);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0; // без черги — відхиляти одразу
    });

    // 2. Політика Sliding Window: максимум 4 запити за 12 секунд (3 сегменти по 4 сек)
    options.AddSlidingWindowLimiter("sliding", opt =>
    {
        opt.PermitLimit = 4;
        opt.Window = TimeSpan.FromSeconds(12);
        opt.SegmentsPerWindow = 3;
        opt.QueueLimit = 0;
    });

    // 3. Політика Token Bucket: кошик на 5 маркерів, поповнюється на 1 кожні 3 секунди
    options.AddTokenBucketLimiter("token_bucket", opt =>
    {
        opt.TokenLimit = 5;
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(3);
        opt.TokensPerPeriod = 1;
        opt.AutoReplenishment = true;
        opt.QueueLimit = 0;
    });
});

builder.Services.AddApiDocs();

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР
// ======================================================================
app.MapApiDocs();

// Важливо: UseRateLimiter має стояти після UseRouting і перед кінцевими endpoint-ами
app.UseRateLimiter();

// ======================================================================
//  3 · ЗАПИТИ
// ======================================================================
app.MapGet("/", () => Results.Content(
    "<h1>Приклад 24: Rate Limiting</h1>" +
    "<ul>" +
    "<li><a href='/api/fixed'>/api/fixed (3 запити / 10 сек)</a></li>" +
    "<li><a href='/api/sliding'>/api/sliding (4 запити / 12 сек)</a></li>" +
    "<li><a href='/api/token'>/api/token (кошик маркерів)</a></li>" +
    "<li><a href='/api/unlimited'>/api/unlimited (без обмежень)</a></li>" +
    "<li><a href='/scalar'>/scalar</a></li>" +
    "</ul>", "text/html"));

// Захищений фіксованим вікном
app.MapGet("/api/fixed", () => Results.Ok(new
{
    message = "Успішний запит (Fixed Window: 3/10s)",
    timestamp = DateTimeOffset.UtcNow
}))
.RequireRateLimiting("fixed")
.WithTags("RateLimiting");

// Захищений ковзним вікном
app.MapGet("/api/sliding", () => Results.Ok(new
{
    message = "Успішний запит (Sliding Window: 4/12s)",
    timestamp = DateTimeOffset.UtcNow
}))
.RequireRateLimiting("sliding")
.WithTags("RateLimiting");

// Захищений кошиком маркерів
app.MapGet("/api/token", () => Results.Ok(new
{
    message = "Успішний запит (Token Bucket: ємність 5, +1 кожні 3с)",
    timestamp = DateTimeOffset.UtcNow
}))
.RequireRateLimiting("token_bucket")
.WithTags("RateLimiting");

// Без лімітів
app.MapGet("/api/unlimited", () => Results.Ok(new
{
    message = "Цей ендпоінт не має жодних обмежень частоти",
    timestamp = DateTimeOffset.UtcNow
}))
.DisableRateLimiting()
.WithTags("RateLimiting");

app.Run();
