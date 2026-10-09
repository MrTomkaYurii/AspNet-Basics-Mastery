using System.ComponentModel.DataAnnotations;
using Common;
using Common.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 11 (Minimal API). Прив'язка параметрів та валідація у Minimal APIs
//
//   1. Джерела прив'язки:
//      [FromRoute], [FromQuery], [FromHeader], [FromBody], [FromServices]
//      та зручний патерн [AsParameters] для об'єднання параметрів у DTO/record.
//
//   2. Валідація:
//      На відміну від MVC-контролерів, Minimal APIs НЕ мають автоматичної валідації
//      DataAnnotations з коробки. Найчистіший архітектурний спосіб реалізації —
//      Endpoint Filters (AddEndpointFilter), які перехоплюють виклик до хендлера.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ
// ======================================================================
builder.Services.AddCatalog();
builder.Services.AddApiDocs();

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР
// ======================================================================
app.MapApiDocs();

// ======================================================================
//  3 · ЗАПИТИ
// ======================================================================
app.MapGet("/", () => "Приклад 11 (Minimal API). Спробуйте /bind/route/42, /bind/query, /bind/header, /bind/search/1, та POST /validate.");

// ── ЧАСТИНА 1: Джерела прив'язки ─────────────────────────────────────
var bindGroup = app.MapGroup("/bind").WithTags("Binding");

// 1.1. Значення з маршруту
bindGroup.MapGet("/route/{id:int}", ([FromRoute] int id) =>
    TypedResults.Ok(new { from = "route", id }));

// 1.2. Query string (nullable для необов'язкових, default значення для page, масив рядків)
bindGroup.MapGet("/query", (
    [FromQuery] string? q,
    [FromQuery] string[]? tags,
    [FromQuery] int page = 1) =>
    TypedResults.Ok(new { from = "query", q, tags = tags ?? [], page }));

// 1.3. Заголовок HTTP (наприклад X-Tenant)
bindGroup.MapGet("/header", ([FromHeader(Name = "X-Tenant")] string tenant) =>
    TypedResults.Ok(new { from = "header", tenant }));

// 1.4. [AsParameters]: упаковує параметри з різних джерел у єдиний об'єкт
bindGroup.MapGet("/search/{page:int}", ([AsParameters] SearchParams search) =>
    TypedResults.Ok(new
    {
        page = search.Page,
        term = search.Term,
        pageSize = search.PageSize
    }));

// ── ЧАСТИНА 2: Валідація через Endpoint Filter ────────────────────────
var validateGroup = app.MapGroup("/validate").WithTags("Validation");

validateGroup.MapPost("/", Results<Ok<string>, ValidationProblem> (
    ProductInput input,
    ICatalog catalog) =>
{
    // Додаткова бізнес-перевірка (наявність категорії)
    if (catalog.FindCategory(input.CategoryId) is null)
    {
        var errors = new Dictionary<string, string[]>
        {
            [nameof(input.CategoryId)] = ["Категорії з таким Id немає."]
        };
        return TypedResults.ValidationProblem(errors);
    }

    return TypedResults.Ok($"Товар успішно прийнято: {input.Name}");
})
.AddEndpointFilter<ValidationFilter<ProductInput>>() // Застосовуємо фільтр валідації
.WithName("ValidateProduct")
.WithSummary("Перевірка валідації вхідної моделі через Endpoint Filter");

app.Run();

// ── Допоміжні типи ───────────────────────────────────────────────────

/// <summary>
/// DTO для демонстрації [AsParameters].
/// Поля автоматично прив'язуються за назвами з маршруту і query.
/// </summary>
public record SearchParams(
    [FromRoute] int Page,
    [FromQuery] string? Term,
    [FromQuery] int PageSize = 20);

/// <summary>
/// Універсальний Endpoint Filter для перевірки DataAnnotations.
/// Якщо модель невалідна, повертає 400 ProblemDetails зі списком помилок,
/// не доходячи до основного тіла обробника.
/// </summary>
public sealed class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var argument = context.Arguments.OfType<T>().FirstOrDefault();
        if (argument is not null)
        {
            var validationContext = new ValidationContext(argument);
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateObject(argument, validationContext, results, validateAllProperties: true))
            {
                var errors = results
                    .GroupBy(r => r.MemberNames.FirstOrDefault() ?? string.Empty)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(r => r.ErrorMessage ?? "Невалідне значення").ToArray());

                return TypedResults.ValidationProblem(errors);
            }
        }

        return await next(context);
    }
}
