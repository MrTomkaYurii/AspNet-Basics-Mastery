using System.Threading.Channels;
using Common.Domain;

namespace Hosting.BackgroundServices;

/// <summary>
/// Потокобезпечна черга задач у пам'яті на базі System.Threading.Channels.
/// Singleton: HTTP-запити кладуть задачі через Writer, а фоновий воркер вичитує через Reader.
/// </summary>
public sealed class BackgroundTaskQueue
{
    private readonly Channel<string> _channel = Channel.CreateBounded<string>(new BoundedChannelOptions(100)
    {
        FullMode = BoundedChannelFullMode.Wait
    });

    public async ValueTask EnqueueAsync(string taskName, CancellationToken ct = default)
        => await _channel.Writer.WriteAsync(taskName, ct);

    public IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct = default)
        => _channel.Reader.ReadAllAsync(ct);
}

/// <summary>
/// Фоновий воркер, що обробляє задачі з черги в асинхронному циклі.
/// Використовує IServiceScopeFactory для створення Scoped-області на кожну задачу!
/// </summary>
public sealed class QueuedTaskProcessor(
    BackgroundTaskQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<QueuedTaskProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("QueuedTaskProcessor запущено. Очікування задач із черги...");

        await foreach (var taskName in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                // ПАТЕРН: Отримання Scoped-сервісу з фонового (Singleton) процесу
                using var scope = scopeFactory.CreateScope();
                var catalog = scope.ServiceProvider.GetRequiredService<ICatalog>();

                logger.LogInformation("Обробка фонової задачі '{Task}': зараз у каталозі {Count} товарів",
                    taskName, catalog.GetProducts().Count);

                // Симуляція корисної роботи
                await Task.Delay(500, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Помилка при обробці фонової задачі '{Task}'", taskName);
            }
        }

        logger.LogInformation("QueuedTaskProcessor завершив роботу.");
    }
}

/// <summary>
/// Періодичний фоновий сервіс на базі PeriodicTimer (сучасна заміна Task.Delay та Timer).
/// </summary>
public sealed class PeriodicCatalogAuditService(
    IServiceScopeFactory scopeFactory,
    ILogger<PeriodicCatalogAuditService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PeriodicCatalogAuditService запущено (інтервал: 10 секунд).");

        // PeriodicTimer не страждає від дрейфу часу й коректно підтримує скасування
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));

        while (!stoppingToken.IsCancellationRequested &&
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var catalog = scope.ServiceProvider.GetRequiredService<ICatalog>();
                var total = catalog.GetProducts().Count;

                logger.LogInformation("[Аудит каталогу] Періодична перевірка: доступно {Count} активних товарів.", total);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Помилка під час періодичного аудиту каталогу.");
            }
        }

        logger.LogInformation("PeriodicCatalogAuditService зупинено.");
    }
}
