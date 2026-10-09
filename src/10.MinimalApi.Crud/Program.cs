using Common;
using Common.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 10 (Minimal API). Повний CRUD на Minimal APIs
//
// Сучасна альтернатива контролерам у .NET 10:
//   • Маршрути реєструються напряму на екземплярі хоста (або в групі MapGroup);
//   • TypedResults замість ControllerBase-методів дає типізовані типи відповідей,
//     що автоматично формують схему OpenAPI без потреби в [ProducesResponseType];
//   • Менше «магії» фреймворку, швидший старт і менше споживання пам'яті.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ  (реєстрація в DI)
// ======================================================================
builder.Services.AddCatalog();
builder.Services.AddApiDocs();

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР  (middleware)
// ======================================================================
app.MapApiDocs();

// ======================================================================
//  3 · ЗАПИТИ  (endpoints через MapGroup та TypedResults)
// ======================================================================
app.MapGet("/", () => Results.Redirect("/products"));

// MapGroup задає спільний префікс шляху та спільні метадані (наприклад, тег OpenAPI)
var products = app.MapGroup("/products")
    .WithTags("Products");

// LIST → 200 OK + масив товарів
products.MapGet("/", (ICatalog catalog) =>
    TypedResults.Ok(catalog.GetProducts()))
    .WithName("GetAllProducts")
    .WithSummary("Отримати список усіх товарів");

// READ → 200 OK або 404 Not Found
products.MapGet("/{id:int}", Results<Ok<Product>, NotFound> (int id, ICatalog catalog) =>
{
    return catalog.FindProduct(id) is { } p
        ? TypedResults.Ok(p)
        : TypedResults.NotFound();
})
.WithName("GetProductById")
.WithSummary("Отримати товар за ідентифікатором");

// CREATE → 201 Created + заголовок Location
products.MapPost("/", Results<CreatedAtRoute<Product>, BadRequest<string>> (ProductInput input, ICatalog catalog) =>
{
    if (catalog.FindCategory(input.CategoryId) is null)
        return TypedResults.BadRequest("Категорії з таким Id немає.");

    var created = catalog.Add(input);
    return TypedResults.CreatedAtRoute(created, "GetProductById", new { id = created.Id });
})
.WithName("CreateProduct")
.WithSummary("Створити новий товар");

// REPLACE (повна заміна) → 200 OK або 404 Not Found. Ідемпотентний.
products.MapPut("/{id:int}", Results<Ok<Product>, NotFound> (int id, ProductInput input, ICatalog catalog) =>
{
    return catalog.Replace(id, input) is { } updated
        ? TypedResults.Ok(updated)
        : TypedResults.NotFound();
})
.WithName("ReplaceProduct")
.WithSummary("Повністю оновити наявний товар");

// DELETE → 204 No Content. Ідемпотентний.
products.MapDelete("/{id:int}", NoContent (int id, ICatalog catalog) =>
{
    catalog.Delete(id);
    return TypedResults.NoContent();
})
.WithName("DeleteProduct")
.WithSummary("Видалити товар за ідентифікатором");

// Допоміжний редирект для категорій
app.MapGet("/categories", (ICatalog catalog) => TypedResults.Ok(catalog.GetCategories()))
    .WithTags("Categories")
    .WithSummary("Отримати список усіх категорій");

app.Run();

public partial class Program;

