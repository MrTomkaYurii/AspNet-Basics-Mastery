using System.Text.Json;
using Common;
using Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 22. Діагностика та Health Checks
//
// У хмарних середовищах (Kubernetes, Docker Swarm, Cloud Run) оркестратор має
// знати стан контейнера. ASP.NET Core має вбудовану підсистему Health Checks:
//   • Liveness probe  (/health/live)  — чи живий процес взагалі? (якщо ні — рестарт контейнера)
//   • Readiness probe (/health/ready) — чи готовий застосунок приймати трафік? (БД, кеш)
//   • Детальний JSON-звіт із метриками та тривалістю перевірок.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ  (реєстрація Health Checks у DI)
// ======================================================================
builder.Services.AddCatalog();

builder.Services.AddHealthChecks()
    // 1. Простий пінг процесу (liveness)
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    // 2. Перевірка доступності сховища даних (readiness)
    .AddCheck<CatalogHealthCheck>("catalog_storage", tags: ["ready"]);

builder.Services.AddApiDocs();

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР
// ======================================================================
app.MapApiDocs();

// Форматер розширеного JSON-звіту для діагностичних інструментів
static Task WriteJsonResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json; charset=utf-8";

    var response = new
    {
        status = report.Status.ToString(),
        totalDurationMs = report.TotalDuration.TotalMilliseconds,
        checks = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString(),
            description = e.Value.Description,
            durationMs = e.Value.Duration.TotalMilliseconds,
            data = e.Value.Data
        })
    };

    return context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
    {
        WriteIndented = true
    }));
}

// Liveness probe: перевіряємо лише внутрішній стан процесу (тег "live")
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = WriteJsonResponse
});

// Readiness probe: перевіряємо всі зовнішні залежності (тег "ready")
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteJsonResponse
});

// Загальний ендпоінт для всіх зареєстрованих перевірок
app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    ResponseWriter = WriteJsonResponse
});

// ======================================================================
//  3 · ЗАПИТИ  (тестове керування станом та API)
// ======================================================================
app.MapGet("/", () => Results.Content(
    "<h1>Приклад 22: Health Checks</h1><ul>" +
    "<li><a href='/health/live'>/health/live (Liveness probe)</a></li>" +
    "<li><a href='/health/ready'>/health/ready (Readiness probe)</a></li>" +
    "<li><a href='/healthz'>/healthz (All checks)</a></li>" +
    "<li><a href='/scalar'>/scalar</a></li></ul>", "text/html"));

// Ендпоінти для симуляції збоїв у рантаймі
app.MapPost("/simulate/fail", () =>
{
    CatalogHealthCheck.IsFailing = true;
    CatalogHealthCheck.IsDegraded = false;
    return Results.Ok("Симульовано аварію сховища (Unhealthy). Перевірте /health/ready.");
}).WithTags("Simulation");

app.MapPost("/simulate/degrade", () =>
{
    CatalogHealthCheck.IsFailing = false;
    CatalogHealthCheck.IsDegraded = true;
    return Results.Ok("Симульовано сповільнення сховища (Degraded). Перевірте /health/ready.");
}).WithTags("Simulation");

app.MapPost("/simulate/restore", () =>
{
    CatalogHealthCheck.IsFailing = false;
    CatalogHealthCheck.IsDegraded = false;
    return Results.Ok("Роботу відновлено (Healthy).");
}).WithTags("Simulation");

app.Run();
