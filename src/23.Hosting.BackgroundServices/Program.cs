using Common;
using Hosting.BackgroundServices;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 23. Фонові задачі та Hosted Services
//
// У реальних застосунках багато операцій (генерація звітів, надсилання email,
// синхронізація, очищення застарілих сесій) виконуються асинхронно у фоні,
// не блокуючи HTTP-відповідь клієнту.
//
// Тут демонструємо:
//   1. BackgroundService — базова абстракція для фонового коду;
//   2. PeriodicTimer (.NET 6+) — точний і надійний таймер для періодичних завдань;
//   3. Черга задач на Channels (System.Threading.Channels) + асинхронний воркер;
//   4. IServiceScopeFactory — як безпечно впроваджувати Scoped-залежності у фоновий Singleton-воркер.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ
// ======================================================================
builder.Services.AddCatalog();

// Реєстрація черги задач як Singleton
builder.Services.AddSingleton<BackgroundTaskQueue>();

// Реєстрація двох фонових воркерів
builder.Services.AddHostedService<QueuedTaskProcessor>();
builder.Services.AddHostedService<PeriodicCatalogAuditService>();

builder.Services.AddApiDocs();

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР
// ======================================================================
app.MapApiDocs();

// ======================================================================
//  3 · ЗАПИТИ
// ======================================================================
app.MapGet("/", () => Results.Content(
    "<h1>Приклад 23: Фонова обробка</h1>" +
    "<p>Періодичний воркер записує аудит у логи кожні 10 секунд.</p>" +
    "<p>Спробуйте POST /queue/job?name=Генерація-звіту для постановки задачі в чергу.</p>" +
    "<p><a href='/scalar'>/scalar</a></p>", "text/html"));

// Додавання фонової задачі через HTTP-запит
app.MapPost("/queue/job", async (string name, BackgroundTaskQueue queue) =>
{
    var taskName = string.IsNullOrWhiteSpace(name) ? $"Задача-{Guid.NewGuid():N}" : name;
    await queue.EnqueueAsync(taskName);

    return Results.Accepted(value: new
    {
        message = "Задачу успішно поставлено в чергу на обробку",
        task = taskName,
        queuedAt = DateTimeOffset.UtcNow
    });
})
.WithTags("Queue")
.WithSummary("Поставити задачу в асинхронну фонову чергу (202 Accepted)");

app.Run();
