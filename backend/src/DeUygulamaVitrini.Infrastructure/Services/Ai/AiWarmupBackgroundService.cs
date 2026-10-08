using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DeUygulamaVitrini.Infrastructure.Services.Ai;

/// <summary>
/// Uygulama ayağa kalktıktan sonra arka planda çalışan ve AI sağlayıcılarına hafif bir
/// warm-up (ısınma) isteği gönderen barındırılan servis (BackgroundService).
/// Web sunucusunun açılışını bloklamaz (non-blocking) ve olası sağlayıcı kesintilerinde
/// uygulamanın normal çalışmasını engellemez (fail-open).
/// </summary>
public class AiWarmupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<AiOptions> _optionsMonitor;
    private readonly ILogger<AiWarmupBackgroundService> _logger;

    public AiWarmupBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<AiOptions> optionsMonitor,
        ILogger<AiWarmupBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = _optionsMonitor.CurrentValue;

        if (!options.Enabled)
        {
            _logger.LogDebug("AiWarmupBackgroundService: AI servisi genel yapılandırmada devre dışı, warm-up çalıştırılmayacak.");
            return;
        }

        if (options.Warmup != null && !options.Warmup.Enabled)
        {
            _logger.LogDebug("AiWarmupBackgroundService: Warmup seçeneği devre dışı bırakıldığı için çalıştırılmadı.");
            return;
        }

        var delaySeconds = options.Warmup?.DelaySeconds ?? 5;
        if (delaySeconds > 0)
        {
            try
            {
                _logger.LogInformation(
                    "AiWarmupBackgroundService başlatıldı. Warm-up işlemi {DelaySeconds} saniye sonra arka planda yürütülecek.",
                    delaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("AiWarmupBackgroundService ilk bekleme sırasında iptal edildi (kapanış).");
                return;
            }
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var warmupService = scope.ServiceProvider.GetRequiredService<IAiWarmupService>();
            await warmupService.WarmupAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("AiWarmupBackgroundService iptal edildi (kapanış).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AiWarmupBackgroundService yürütülürken beklenmeyen bir hata oluştu. Sunucu normal çalışmaya devam ediyor.");
        }
    }
}
