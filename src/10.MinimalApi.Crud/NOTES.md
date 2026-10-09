# 10. Minimal API CRUD

## Що демонструємо

Повний набір CRUD-операцій (`Create`, `Read`, `Update`, `Delete`) над сутністю товару за допомогою парадигми **Minimal APIs** у .NET 10.

## Ключові концепції

### 1. Minimal APIs проти Контролерів MVC
- **Контролери:** Вимагають успадкування від `ControllerBase`, атрибутів `[ApiController]`, `[Route]`, механізму reflection для сканування збірок і реєстрації фільтрів.
- **Minimal APIs:** Реєструються безпосередньо в конвеєрі через `app.MapGet()`, `app.MapPost()` тощо. Менше абстракцій, прямий виклик обробника, швидший старт і менше споживання пам'яті (ідеально для мікросервісів та cloud-native застосунків).

### 2. Організація маршрутів через `MapGroup`
Метод `app.MapGroup("/products")` дозволяє:
- Задати спільний префікс шляху без дублювання рядків.
- Застосувати спільні метадані (наприклад, `.WithTags("Products")` для OpenAPI).
- Застосувати спільні endpoint-фільтри або авторизацію до всієї групи одночасно.

### 3. Строга типізація відповідей із `TypedResults`
Замість «нетипізованого» `Results.Ok()` або `Results.NotFound()`, у .NET використовується `TypedResults`:
- Повертає конкретний тип структури `Results<Ok<Product>, NotFound>`.
- Надає **статичну типізацію** при компіляції (компілятор гарантує, що метод може повернути лише 200 або 404).
- **Автоматично генерує метадані OpenAPI**: Swagger і Scalar точно знають можливі статус-коди й схеми тіла без ручних атрибутів `[ProducesResponseType]`.
- Спрощує unit-тестування: повернутий об'єкт можна перевірити без виклику HTTP-контексту.

## Спробуйте самі

1. Запустіть проєкт:
   ```bash
   dotnet run --project src/10.MinimalApi.Crud
   ```
2. Відкрийте браузер на `http://localhost:5110/scalar` або `http://localhost:5110/swagger`.
3. Виконайте запити з `10.MinimalApi.Crud.http`:
   - `GET /products` — отримання списку.
   - `POST /products` — створення з поверненням заголовка `Location`.
   - `DELETE /products/3` — перевірка ідемпотентного 204.

## Посилання

- Minimal APIs overview: <https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/overview>
- TypedResults in Minimal APIs: <https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/responses#typedresults-vs-results>
- Route groups: <https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/route-handlers#route-groups>
