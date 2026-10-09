# 22. Діагностика та Health Checks

## Що демонструємо

Вбудовану систему діагностики працездатності застосунку (**Health Checks**) в ASP.NET Core:
1. Поділ на **Liveness** та **Readiness** проби для контейнеризації (Kubernetes / Docker).
2. Написання власного перевіряльника `IHealthCheck` з поверненням кастомних метрик.
3. Форматування структурованого JSON-звіту з тривалістю та статусами кожної залежності.
4. Симуляцію відмови залежності та спостереження за поведінкою статус-кодів (200 OK проти 503 Service Unavailable).

## Ключові концепції

### 1. Чому потрібні окремі проби?
- **Liveness probe (`/health/live`):** Відповідає на питання «Чи живий процес?». Якщо виникає deadlock або процес завис, проба не повертає 200, і Kubernetes перезавантажує контейнер (restart). Не повинна залежати від бази даних чи зовнішніх сервісів!
- **Readiness probe (`/health/ready`):** Відповідає на питання «Чи готовий застосунок приймати користувацький трафік?». Перевіряє підключення до БД, черг повідомлень та сторонніх API. Якщо БД недоступна, балансувальник тимчасово припиняє надсилати запити на цей под, але НЕ перезапускає його.

### 2. Тегування перевірок (`tags`)
При реєстрації перевірок можна вказувати теги:
```csharp
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    .AddCheck<CatalogHealthCheck>("catalog", tags: ["ready"]);
```
При мапінгу ендпоінта фільтруємо їх через `Predicate`:
```csharp
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});
```

### 3. Статус-коди
- `Healthy` → 200 OK
- `Degraded` → 200 OK (застосунок працює, але з проблемами)
- `Unhealthy` → 503 Service Unavailable

## Спробуйте самі

1. Запустіть проєкт:
   ```bash
   dotnet run --project src/22.Diagnostics.HealthChecks
   ```
2. Виконайте `GET /health/ready` — статус `Healthy` (200 OK).
3. Зробіть `POST /simulate/fail` — симуляція падіння сховища.
4. Знову виконайте `GET /health/ready` — поверне 503 Service Unavailable зі статусом `Unhealthy`.
5. Виконайте `GET /health/live` — поверне 200 OK, бо сам процес працює бездоганно.
6. Зробіть `POST /simulate/restore` і переконайтеся, що готовність відновилася.

## Посилання

- Health checks in ASP.NET Core: <https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks>
- Kubernetes Liveness and Readiness Probes: <https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/>
