using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DeUygulamaVitrini.Infrastructure.Services.Ai;

/// <summary>
/// Proje Kütüphanesi otomatik anlamsal indeks yaşam döngüsü arka plan işçisi (BackgroundService).
/// SQL veri tabanındaki Proje verisi tek doğruluk kaynağı (Source of Truth), anlamsal indeks parçaları ise
/// türetilmiş veridir (Derived Data). Bu işçi, arka planda eksik veya güncellenmiş (missing/stale) projeleri
/// sınırlandırılmış gruplar (bounded batch) halinde tespit eder ve embedding indeksini eşitler.
/// </summary>
public class SemanticIndexBackgroundWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<AiOptions> _optionsMonitor;
    private readonly ILogger<SemanticIndexBackgroundWorker> _logger;
    private readonly SemaphoreSlim _reconciliationLock = new(1, 1);

    public SemanticIndexBackgroundWorker(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<AiOptions> optionsMonitor,
        ILogger<SemanticIndexBackgroundWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = _optionsMonitor.CurrentValue;
        var initialDelaySeconds = options.SemanticIndex?.InitialDelaySeconds ?? 5;

        _logger.LogInformation(
            "SemanticIndexBackgroundWorker başlatıldı. İlk eşitleme {Delay} saniye sonra başlayacak.",
            initialDelaySeconds);

        // API'nin hızla ayağa kalkması ve hazır hale gelmesi için ilk gecikme
        if (initialDelaySeconds > 0)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(initialDelaySeconds), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("SemanticIndexBackgroundWorker durduruluyor (ilk bekleme sırasında iptal).");
                return;
            }
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var currentOptions = _optionsMonitor.CurrentValue;
            var intervalSeconds = currentOptions.SemanticIndex?.ReconciliationIntervalSeconds ?? 30;
            var batchSize = currentOptions.SemanticIndex?.BatchSize ?? 5;
            var isEnabled = currentOptions.Enabled && (currentOptions.SemanticIndex?.BackgroundIndexingEnabled ?? true);

            if (isEnabled)
            {
                await RunReconciliationCycleAsync(batchSize, stoppingToken);
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Max(5, intervalSeconds)), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("SemanticIndexBackgroundWorker başarıyla sonlandırıldı.");
    }

    private async Task RunReconciliationCycleAsync(int batchSize, CancellationToken cancellationToken)
    {
        if (!await _reconciliationLock.WaitAsync(0, cancellationToken))
        {
            _logger.LogDebug("Önceki eşitleme döngüsü henüz tamamlanmadığı için bu tur atlandı.");
            return;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var indexService = scope.ServiceProvider.GetRequiredService<IProjectKnowledgeIndexService>();

            var result = await indexService.ReconcileBatchAsync(batchSize, cancellationToken);

            if (result.ProviderUnavailable)
            {
                _logger.LogDebug("Arka plan indeksleme: AI sağlayıcısı devre dışı veya erişilemez durumda.");
            }
            else if (result.ProjectsProcessedInBatch > 0 || result.ChunksDeleted > 0)
            {
                _logger.LogInformation(
                    "Arka plan indeksleme döngüsü tamamlandı ({Duration} ms): İşlenen={Processed}, Güncellenen/Oluşturulan={Updated}, Değişmeyen={Unchanged}, Silinen={Deleted}, Başarısız={Failed}",
                    result.DurationMs, result.ProjectsProcessedInBatch, result.ChunksCreatedOrUpdated, result.ChunksUnchanged, result.ChunksDeleted, result.FailedProjectsCount);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Normal kapanma senaryosu
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Arka plan indeks eşitleme döngüsünde beklenmeyen bir hata oluştu. Sunucu çalışmaya devam ediyor.");
        }
        finally
        {
            _reconciliationLock.Release();
        }
    }

    public override void Dispose()
    {
        _reconciliationLock.Dispose();
        base.Dispose();
    }
}
