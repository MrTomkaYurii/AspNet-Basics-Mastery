using Common.Domain;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Diagnostics.HealthChecks;

/// <summary>
/// Кастомна перевірка працездатності залежності (сховища товарів).
/// У реальному застосунку тут виконується запит до БД (SELECT 1) або зовнішнього сервісу.
/// </summary>
public sealed class CatalogHealthCheck(ICatalog catalog) : IHealthCheck
{
    // Прапорець для симуляції збою при тестуванні
    public static bool IsDegraded { get; set; }
    public static bool IsFailing { get; set; }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (IsFailing)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                description: "Сховище недоступне (симуляція аварії)."));
        }

        if (IsDegraded)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                description: "Сховище відповідає з підвищеною затримкою."));
        }

        var count = catalog.GetProducts().Count;
        return Task.FromResult(HealthCheckResult.Healthy(
            description: $"Сховище готове до роботи, доступно {count} товарів.",
            data: new Dictionary<string, object>
            {
                ["productCount"] = count,
                ["timestamp"] = DateTimeOffset.UtcNow
            }));
    }
}
