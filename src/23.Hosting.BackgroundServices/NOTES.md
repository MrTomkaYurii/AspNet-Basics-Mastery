# 23. Фонова обробка: Hosted Services та BackgroundWorker

## Що демонструємо

Реалізацію фонових задач (background processing) в ASP.NET Core:
1. Використання абстракції `BackgroundService` (`IHostedService`).
2. Надійні періодичні інтервали за допомогою `PeriodicTimer` у .NET.
3. Побудову внутрішньої черги задач на базі `System.Threading.Channels.Channel<T>`.
4. Вирішення проблеми «Captive Dependency»: отримання `Scoped`-сервісів у довготривалому `Singleton`-процесі через `IServiceScopeFactory`.

## Ключові концепції

### 1. `IHostedService` та `BackgroundService`
Будь-який клас, зареєстрований через `builder.Services.AddHostedService<T>()`, запускається при старті хоста (`StartAsync`) і зупиняється при завершенні роботи застосунку (`StopAsync`). `BackgroundService` — зручний базовий клас, де достатньо перевизначити лише один метод `ExecuteAsync(CancellationToken stoppingToken)`.

### 2. Пастка Captive Dependency (Захоплення залежностей)
Фонові сервіси є **Singleton** за своєю природою (один процес на весь життєвий цикл застосунку). Якщо напряму заінжектити у його конструктор `Scoped`-сервіс (наприклад, контекст бази даних `DbContext` або наш `ICatalog`):
- У Development рантайм кине виняток на етапі перевірки валідації областей.
- У Production `DbContext` ніколи б не звільнявся, накопичуючи кеш трекінгу сутностей і спричиняючи витік пам'яті та помилки паралельного доступу.

**Правильне рішення:** Впроваджувати `IServiceScopeFactory`, створювати нову область під кожну ітерацію або задачу через `using var scope = scopeFactory.CreateScope();` і резолвити потрібні залежності з `scope.ServiceProvider`.

### 3. Черги задач через Channels
Замість `ConcurrentQueue<T>` із блокуючими або опитувальними циклами, у сучасному .NET використовується `System.Threading.Channels`:
- Підтримує повністю асинхронний `await foreach (var item in queue.ReadAllAsync(ct))`.
- Не навантажує процесор під час очікування нових повідомлень.
- Підтримує обмеження місткості (`BoundedChannel`) для захисту від переповнення пам'яті (backpressure).

## Спробуйте самі

1. Запустіть проєкт:
   ```bash
   dotnet run --project src/23.Hosting.BackgroundServices
   ```
2. Поспостерігайте за логами в терміналі: кожні 10 секунд спрацьовуватиме `PeriodicCatalogAuditService`.
3. Виконайте `POST /queue/job?name=Генерація-звіту` — endpoint миттєво поверне 202 Accepted, а в консолі з'явиться повідомлення про обробку задачі воркером `QueuedTaskProcessor`.
4. Натисніть `Ctrl+C` — простежте, як токен `stoppingToken` коректно зупиняє цикли обробки без аварійних падінь.

## Посилання

- Background tasks with hosted services: <https://learn.microsoft.com/aspnet/core/fundamentals/host/hosted-services>
- An introduction to System.Threading.Channels: <https://devblogs.microsoft.com/dotnet/an-introduction-to-system-threading-channels/>
