namespace NexTech.HackerNews.Feed.Api.Workers
{
    using Microsoft.ApplicationInsights;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using NexTech.HackerNews.Feed.Application.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Configuration settings for cache warming behavior.
    /// </summary>
    public class CacheWarmerOptions
    {
        /// <summary>
        /// Interval between refreshes. Default: 5 minutes.
        /// </summary>
        public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Number of top stories to refresh and cache.
        /// </summary>
        public int TopCount { get; set; } = 50;

        /// <summary>
        /// Enables or disables the cache warmer background service.
        /// </summary>
        public bool Enabled { get; set; } = true;
    }

    /// <summary>
    /// Background worker that periodically refreshes the top HackerNews stories cache.
    /// Ensures that the API always serves low-latency, pre-warmed data.
    /// </summary>
    public class TopStoriesCacheWarmer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TopStoriesCacheWarmer> _logger;
        private readonly IOptionsMonitor<CacheWarmerOptions> _options;
        private readonly TelemetryClient? _telemetryClient;

        public TopStoriesCacheWarmer(
            IServiceScopeFactory scopeFactory,
            ILogger<TopStoriesCacheWarmer> logger,
            IOptionsMonitor<CacheWarmerOptions> options,
            TelemetryClient? telemetryClient = null)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _options = options;
            _telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Executes the cache refresh loop until the host shuts down.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var opts = _options.CurrentValue;

            _logger.LogInformation("TopStoriesCacheWarmer starting. Enabled={Enabled}, Interval={Interval} min, TopCount={TopCount}",
                opts.Enabled, opts.RefreshInterval.TotalMinutes, opts.TopCount);

            // Disable warmup if disabled in configuration
            if (!opts.Enabled)
            {
                _logger.LogInformation("CacheWarmer is disabled in configuration. Skipping execution.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var newsService = scope.ServiceProvider.GetRequiredService<INewsStoryService>();

                    _logger.LogInformation("Starting cache warm-up for top {Count} stories...", opts.TopCount);
                    var startTime = DateTimeOffset.UtcNow;

                    await newsService.RefreshTopStoriesCacheAsync(opts.TopCount, stoppingToken);

                    var duration = DateTimeOffset.UtcNow - startTime;
                    _logger.LogInformation("Cache warm-up completed successfully in {Duration} seconds.", duration.TotalSeconds);

                    _telemetryClient?.TrackEvent("CacheWarmupSuccess", new Dictionary<string, string>
                    {
                        ["DurationSeconds"] = duration.TotalSeconds.ToString("F1"),
                        ["RefreshedAt"] = DateTimeOffset.UtcNow.ToString("O"),
                        ["StoryCount"] = opts.TopCount.ToString()
                    });
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("TopStoriesCacheWarmer canceled.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during TopStoriesCacheWarmer execution.");
                    _telemetryClient?.TrackException(ex, new Dictionary<string, string>
                    {
                        ["Source"] = nameof(TopStoriesCacheWarmer),
                        ["Message"] = ex.Message
                    });
                }

                try
                {
                    await Task.Delay(opts.RefreshInterval, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }

            _logger.LogInformation("TopStoriesCacheWarmer stopped gracefully.");
        }
    }
}