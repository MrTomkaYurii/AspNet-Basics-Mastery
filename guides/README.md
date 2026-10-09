# Повний довідник та навчальні гайди: «Основи ASP.NET Core»

Ласкаво просимо до каталогу детальних навчальних гайдів курсу **«Основи ASP.NET Core на .NET 10»**.

Кожен проєкт у цьому solution є незалежним прикладом із власною точкою входу, коментарями в коді, готовим файлом `.http`-запитів та детальним теоретично-практичним поясненням у цьому каталозі `guides/`.

---

## Навігатор по навчальних модулях

### Модуль 0. Спільна основа
| Гайд | Проєкт | Опис |
|---|---|---|
| [00. Спільна основа](00.Common.md) | `src/00.Common` | Доменні моделі (`Product`, `Category`), потокобезпечне сховище в пам'яті (`System.Threading.Lock`), помічник паралельної документації Swagger + Scalar. |

### Модуль 1. Хостинг і платформа
| Гайд | Проєкт | Опис |
|---|---|---|
| [01. Мінімальний хост](01.Hosting.Minimal.md) | `src/01.Hosting.Minimal` | Що таке `Host` та `WebApplication`, дві фази (`builder` vs `app`), вбудовані DI/конфігурація/логування, graceful shutdown. |
| [02. Сервери та середовища](02.Hosting.ServersAndEnvironment.md) | `src/02.Hosting.ServersAndEnvironment` | Kestrel, ліміти сервера, робота з `IWebHostEnvironment` (Development vs Production), ієрархія налаштувань. |

### Модуль 2. Конвеєр обробки запиту (Middleware Pipeline)
| Гайд | Проєкт | Опис |
|---|---|---|
| [03. Конвеєр з нуля](03.Pipeline.FromScratch.md) | `src/03.Pipeline.FromScratch` | Делегат `next`, модель «цибулина», обробка на вході й виході, коротке замикання (short-circuit), робота з сирим `HttpContext`. |
| [04. Власні Middleware у класах](04.Pipeline.CustomMiddleware.md) | `src/04.Pipeline.CustomMiddleware` | Convention-based проти `IMiddleware`, таймінг запиту, наскрізний заголовок `X-Correlation-ID`. |
| [05. Наскрізна функціональність](05.Pipeline.CrossCutting.md) | `src/05.Pipeline.CrossCutting` | Критичність порядку реєстрації middleware, `ProblemDetails` (RFC 9457), статичні файли, CORS. |
| [06. Розгалуження конвеєра](06.Pipeline.Branching.md) | `src/06.Pipeline.Branching` | `app.Map`, `app.MapWhen` та `app.UseWhen`: відмінності в термінальності та поверненні в основний конвеєр. |

### Модуль 3. Dependency Injection і конфігурація
| Гайд | Проєкт | Опис |
|---|---|---|
| [07. Час життя сервісів у DI](07.DependencyInjection.Lifetimes.md) | `src/07.DependencyInjection.Lifetimes` | `Transient`, `Scoped`, `Singleton` на реальному порівнянні ID, пастка Captive Dependency, фабрика `IServiceScopeFactory`. |
| [08. Конфігурація та Options](08.Configuration.Options.md) | `src/08.Configuration.Options` | Патерн Options, `IOptions`, `IOptionsSnapshot`, `IOptionsMonitor`, валідація на старті (`ValidateOnStart`). |

### Модуль 4. Маршрутизація
| Гайд | Проєкт | Опис |
|---|---|---|
| [09. Endpoint Routing](09.Routing.Endpoints.md) | `src/09.Routing.Endpoints` | Двоетапна маршрутизація (`UseRouting` vs `UseEndpoints`), шаблони маршрутів, обмеження (constraints), `LinkGenerator`. |

### Модуль 5. Web API: Контролери та Minimal APIs
| Гайд | Проєкт | Опис |
|---|---|---|
| [10. CRUD на MVC-контролерах](10.Mvc.Crud.md) | `src/10.Mvc.Crud` | Класичний підхід MVC, `ControllerBase`, `ActionResult<T>`, статус-коди та заголовок `Location`. |
| [10. CRUD на Minimal APIs](10.MinimalApi.Crud.md) | `src/10.MinimalApi.Crud` | Сучасний підхід у .NET 10: `MapGroup`, строга типізація через `TypedResults`, автоматична генерація OpenAPI без зайвих атрибутів. |
| [11. Прив'язка та валідація у MVC](11.Mvc.BindingAndValidation.md) | `src/11.Mvc.BindingAndValidation` | Джерела прив'язки (`[FromRoute]`, `[FromQuery]`, тощо), автоматична валідація 400 від `[ApiController]`, `ModelState`. |
| [11. Прив'язка та валідація у Minimal APIs](11.MinimalApi.BindingAndValidation.md) | `src/11.MinimalApi.BindingAndValidation` | Патерн `[AsParameters]`, валідація вхідних даних через `Endpoint Filters` (`IEndpointFilter`). |

### Модуль 6. MVC / Контролери (глибше)
| Гайд | Проєкт | Опис |
|---|---|---|
| [12. Глибокий розбір контролерів](12.Mvc.Controllers.md) | `src/12.Mvc.Controllers` | Детальний розбір механіки `[ApiController]`, вивід джерел прив'язки, робота з `ActionResult<T>`. |
| [13. Фільтри MVC](13.Mvc.Filters.md) | `src/13.Mvc.Filters` | Повний життєвий цикл фільтрів: Authorization, Resource, Action, Exception, Result, відмінність від middleware. |

### Модуль 7. REST по-справжньому
| Гайд | Проєкт | Опис |
|---|---|---|
| [14. Ресурси та HTTP-дієслова](14.Rest.ResourcesAndVerbs.md) | `src/14.Rest.ResourcesAndVerbs` | Стандарт RFC 9110, ресурсно-орієнтований дизайн URI, властивості методів Safe та Idempotent. |
| [15. Представлення та блокування](15.Rest.Representations.md) | `src/15.Rest.Representations` | Content negotiation, валідація кешу `ETag` + `304 Not Modified`, оптимістичне блокування `If-Match` + `412`, JSON Patch (RFC 6902). |
| [16. Колекції, пагінація та помилки](16.Rest.CollectionsAndErrors.md) | `src/16.Rest.CollectionsAndErrors` | Пагінація, фільтрація через query string, структура конверта `Page<T>`, кастомні розширення `ProblemDetails`. |
| [17. Версіонування API](17.Rest.Versioning.md) | `src/17.Rest.Versioning` | Бібліотека `Asp.Versioning`: 4 способи передачі версії (URL, query, header, media-type), застарілі версії (deprecation). |
| [18. Гіпермедіа (HATEOAS)](18.Rest.Hypermedia.md) | `src/18.Rest.Hypermedia` | Рівень 3 моделі Річардсона, динамічні посилання `_links` на основі бізнес-стану ресурсу кошика. |

### Модуль 8. Документація і Тестування
| Гайд | Проєкт | Опис |
|---|---|---|
| [19. OpenAPI, Swagger та Scalar](19.OpenApi.Swagger.md) | `src/19.OpenApi.Swagger` | Вбудована генерація специфікації OpenAPI у .NET 10, Document/Schema Transformers, паралельна робота Swagger UI та Scalar. |
| [20. Інтеграційні тести](20.Testing.md) | `tests/20.Testing` | Підняття застосунку в пам'яті через `WebApplicationFactory<Program>`, тестування статус-кодів, валідації та ідемпотентності. |

### Модуль 9. Безпека
| Гайд | Проєкт | Опис |
|---|---|---|
| [21. Основи безпеки: AuthN та AuthZ](21.Security.AuthBasics.md) | `src/21.Security.AuthBasics` | Автентифікація vs Авторизація, JWT Bearer токени, політики та ролі доступу, статус-коди `401 Unauthorized` проти `403 Forbidden`. |

### Модуль 10. Виробнича надійність та продуктивність
| Гайд | Проєкт | Опис |
|---|---|---|
| [22. Діагностика та Health Checks](22.Diagnostics.HealthChecks.md) | `src/22.Diagnostics.HealthChecks` | Liveness (`/health/live`) та Readiness (`/health/ready`) проби для Kubernetes, власні `IHealthCheck`, структурований JSON-звіт. |
| [23. Фонові задачі та Hosted Services](23.Hosting.BackgroundServices.md) | `src/23.Hosting.BackgroundServices` | Фонова обробка: `BackgroundService`, `PeriodicTimer`, черги задач на `System.Threading.Channels`, безпечний виклик `Scoped` сервісів. |
| [24. Обмеження частоти (Rate Limiting)](24.Performance.RateLimiting.md) | `src/24.Performance.RateLimiting` | Вбудований захист: Fixed Window, Sliding Window, Token Bucket, обробка відхилених запитів `429 Too Many Requests` та заголовок `Retry-After`. |

---

## Шпаргалка портів та адреси запуску

Кожен приклад слухає власний фіксований локальний порт, тож ви можете тримати одночасно запущеними кілька різних прикладів:

| Проєкт | Порт | Стартовий URL | Головні ендпоінти для перевірки |
|---|:---:|---|---|
| `01.Hosting.Minimal` | `5001` | `http://localhost:5001/` | `/env`, `/products`, `/shutdown` |
| `02.Hosting.ServersAndEnvironment` | `5002` | `http://localhost:5002/` | `/env`, `/server`, `/config`, `/dev/ping` |
| `03.Pipeline.FromScratch` | `5003` | `http://localhost:5003/` | `/`, `/secret` (з `X-Api-Key`), `/nope` |
| `04.Pipeline.CustomMiddleware` | `5004` | `http://localhost:5004/scalar` | `/` (з `X-Correlation-ID`) |
| `05.Pipeline.CrossCutting` | `5005` | `http://localhost:5005/index.html` | `/api/data`, `/api/fail`, `/api/missing` |
| `06.Pipeline.Branching` | `5006` | `http://localhost:5006/` | `/admin`, `/api/ping`, `X-Legacy-Client` |
| `07.DependencyInjection.Lifetimes` | `5007` | `http://localhost:5007/scalar` | `/ids`, `/scopes` |
| `08.Configuration.Options` | `5008` | `http://localhost:5008/scalar` | `/sources`, `/options/io`, `/options/snapshot`, `/options/monitor` |
| `09.Routing.Endpoints` | `5009` | `http://localhost:5009/scalar` | `/products/1`, `/products/featured`, `/categories/laptops`, `/files/a/b` |
| `10.Mvc.Crud` | `5010` | `http://localhost:5010/scalar` | `/products`, `/products/1`, `/categories` |
| `10.MinimalApi.Crud` | `5110` | `http://localhost:5110/scalar` | `/products`, `/products/1`, `/categories` |
| `11.Mvc.BindingAndValidation` | `5011` | `http://localhost:5011/scalar` | `/bind/route/1`, `/bind/query`, `/bind/header`, `/validate` |
| `11.MinimalApi.BindingAndValidation` | `5111` | `http://localhost:5111/scalar` | `/bind/route/1`, `/bind/query`, `/bind/search/1`, `/validate` |
| `12.Mvc.Controllers` | `5012` | `http://localhost:5012/scalar` | `/api/products`, `/api/products/1` |
| `13.Mvc.Filters` | `5013` | `http://localhost:5013/scalar` | `/api/demo/ok`, `/api/demo/boom` |
| `14.Rest.ResourcesAndVerbs` | `5014` | `http://localhost:5014/scalar` | `GET/POST/PUT/DELETE /products` |
| `15.Rest.Representations` | `5015` | `http://localhost:5015/scalar` | `/products/1` (`ETag`, `If-Match`, `PATCH`) |
| `16.Rest.CollectionsAndErrors` | `5016` | `http://localhost:5016/scalar` | `/products?page=1&pageSize=10`, `/products/999` |
| `17.Rest.Versioning` | `5017` | `http://localhost:5017/scalar` | `/v1/products`, `/v2/products`, `X-Api-Version: 2.0` |
| `18.Rest.Hypermedia` | `5018` | `http://localhost:5018/` | `/`, `/cart`, `/cart/items`, `/cart/checkout` |
| `19.OpenApi.Swagger` | `5019` | `http://localhost:5019/` | `/swagger`, `/scalar`, `/openapi/v1.json` |
| `20.Testing` | — | `dotnet test` | Інтеграційні тести xUnit у пам'яті |
| `21.Security.AuthBasics` | `5021` | `http://localhost:5021/scalar` | `/token`, `/me`, `/admin` |
| `22.Diagnostics.HealthChecks` | `5022` | `http://localhost:5022/scalar` | `/health/live`, `/health/ready`, `/healthz`, `/simulate/fail` |
| `23.Hosting.BackgroundServices` | `5023` | `http://localhost:5023/scalar` | `/queue/job`, періодичний аудит у консолі |
| `24.Performance.RateLimiting` | `5024` | `http://localhost:5024/scalar` | `/api/fixed`, `/api/sliding`, `/api/token`, `/api/unlimited` |

---

## Рекомендовані треки проходження курсу

### 🎓 Трек 1: «Основи для початківців» (Фундамент платформо-знавства)
1. [`01.Hosting.Minimal`](01.Hosting.Minimal.md) $\to$ [`02.Hosting.ServersAndEnvironment`](02.Hosting.ServersAndEnvironment.md) (Як влаштований хост)
2. [`03.Pipeline.FromScratch`](03.Pipeline.FromScratch.md) $\to$ [`04.Pipeline.CustomMiddleware`](04.Pipeline.CustomMiddleware.md) (Конвеєр middleware)
3. [`07.DependencyInjection.Lifetimes`](07.DependencyInjection.Lifetimes.md) $\to$ [`08.Configuration.Options`](08.Configuration.Options.md) (DI та конфігурація)
4. [`09.Routing.Endpoints`](09.Routing.Endpoints.md) (Маршрутизація)

### 🚀 Трек 2: «Сучасна розробка Web API» (MVC проти Minimal APIs)
1. [`10.Mvc.Crud`](10.Mvc.Crud.md) порівняти з [`10.MinimalApi.Crud`](10.MinimalApi.Crud.md)
2. [`11.Mvc.BindingAndValidation`](11.Mvc.BindingAndValidation.md) порівняти з [`11.MinimalApi.BindingAndValidation`](11.MinimalApi.BindingAndValidation.md)
3. [`12.Mvc.Controllers`](12.Mvc.Controllers.md) та [`13.Mvc.Filters`](13.Mvc.Filters.md)
4. [`19.OpenApi.Swagger`](19.OpenApi.Swagger.md) (Документація)
5. [`20.Testing`](20.Testing.md) (Інтеграційні тести WebApplicationFactory)

### 💎 Трек 3: «Професійний REST та Продакшен» (Enterprise & Cloud-Native)
1. [`14.Rest.ResourcesAndVerbs`](14.Rest.ResourcesAndVerbs.md) (Семантика дієслів та ідемпотентність)
2. [`15.Rest.Representations`](15.Rest.Representations.md) (ETag, If-Match, RFC 6902 JSON Patch)
3. [`16.Rest.CollectionsAndErrors`](16.Rest.CollectionsAndErrors.md) (RFC 9457 ProblemDetails, пагінація)
4. [`17.Rest.Versioning`](17.Rest.Versioning.md) та [`18.Rest.Hypermedia`](18.Rest.Hypermedia.md)
5. [`21.Security.AuthBasics`](21.Security.AuthBasics.md) (JWT AuthN & AuthZ)
6. [`22.Diagnostics.HealthChecks`](22.Diagnostics.HealthChecks.md) (Kubernetes Liveness/Readiness)
7. [`23.Hosting.BackgroundServices`](23.Hosting.BackgroundServices.md) (Channels, PeriodicTimer, Captive Dependency)
8. [`24.Performance.RateLimiting`](24.Performance.RateLimiting.md) (Token Bucket, 429 Too Many Requests)

---

## Швидкий запуск та перевірка

```bash
# 1. Зібрати всі 27 проєктів
dotnet build

# 2. Запустити інтеграційні тести
dotnet test

# 3. Запустити конкретний приклад (наприклад Minimal API CRUD):
dotnet run --project src/10.MinimalApi.Crud

# 4. Відкрити інтерактивну документацію в браузері:
#    http://localhost:5110/scalar
```
