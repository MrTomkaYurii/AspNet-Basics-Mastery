# 11. Minimal API: Binding and Validation

## Що демонструємо

Способи прив'язки вхідних параметрів у **Minimal APIs** (`[FromRoute]`, `[FromQuery]`, `[FromHeader]`, `[AsParameters]`), а також реалізацію валідації вхідних даних за допомогою **Endpoint Filters** (`IEndpointFilter`) та повернення стандартизованого `ValidationProblemDetails` (RFC 9457).

## Ключові концепції

### 1. Джерела прив'язки (Binding Sources)
У Minimal APIs параметри обробника аналізуються платформою за наступними правилами:
- Параметри збігаються з назвами сегментів маршруту `{id}` → прив'язуються з маршруту (`[FromRoute]`).
- Прості типи (`string`, `int`, масиви), яких немає в шаблоні маршруту → зчитуються з query string (`[FromQuery]`).
- Складні класи/записи без спеціальних атрибутів → десеріалізуються з JSON тіла запиту (`[FromBody]`).
- Сервіси, зареєстровані в DI-контейнері → автоматично резолвляться (`[FromServices]`).
- Для заголовків використовується явний атрибут `[FromHeader(Name = "...")]`.

### 2. Патерн `[AsParameters]`
Замість передачі десятка окремих параметрів у метод-хендлер, можна створити єдиний `record` або `class` і позначити його атрибутом `[AsParameters]`. Платформа автоматично наповнить його поля з відповідних джерел (маршруту, query, заголовків, сервісів). Це робить сигнатуру чистішою та спрощує тестування.

### 3. Валідація: відмінність від MVC
В MVC-контролерах атрибут `[ApiController]` автоматично запускає валідацію `ModelState` і повертає 400.
**У Minimal APIs за замовчуванням автоматичної валідації немає!**
Архітектурний підхід у .NET — використання **Endpoint Filters** (`AddEndpointFilter<ValidationFilter<T>>()`):
1. Фільтр виконується ДО виклику цільового коду endpoint-а.
2. Фільтр знаходить аргумент відповідного типу і викликає `Validator.TryValidateObject(...)`.
3. У разі помилок фільтр повертає `TypedResults.ValidationProblem(errors)`, роблячи short-circuit і запобігаючи виконанню непотрібного коду.

## Спробуйте самі

1. Запустіть проєкт:
   ```bash
   dotnet run --project src/11.MinimalApi.BindingAndValidation
   ```
2. Відкрийте `http://localhost:5111/scalar`.
3. Виконайте запити з `11.MinimalApi.BindingAndValidation.http`:
   - `GET /bind/query?q=phone&tags=sale&tags=electronics` — перевірте прив'язку списків.
   - `GET /bind/search/3?term=laptop&pageSize=50` — перевірте роботу `[AsParameters]`.
   - `POST /validate` з невалідним тілом — перевірте, як `EndpointFilter` перехоплює помилку та повертає 400.

## Посилання

- Parameter binding in Minimal APIs: <https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/parameter-binding>
- Filters in Minimal APIs: <https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/min-api-filters>
- Validation in Minimal APIs: <https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-and-validation>
