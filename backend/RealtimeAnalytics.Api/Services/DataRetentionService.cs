using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RealtimeAnalytics.Api.Data;

namespace RealtimeAnalytics.Api.Services
{
    public class DataRetentionService : BackgroundService
    {
        private readonly ReadingStore _store;
        private readonly IConfiguration _cfg;
        private readonly ILogger<DataRetentionService> _logger;

        public DataRetentionService(ReadingStore store, IConfiguration cfg, ILogger<DataRetentionService> logger)
        {
            _store = store;
            _cfg = cfg;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var hours = _cfg.GetSection("Retention").GetValue<int>("PurgeAfterHours", 24);
            var interval = TimeSpan.FromMinutes(5);
            while (!stoppingToken.IsCancellationRequested)
            {
                var cutoff = DateTimeOffset.UtcNow.AddHours(-hours);
                var removed = _store.PurgeOlderThan(cutoff);
                if (removed > 0)
                    _logger.LogInformation("Purged {Removed} old readings (cutoff {Cutoff})", removed, cutoff);
                await Task.Delay(interval, stoppingToken);
            }
        }
    }
}