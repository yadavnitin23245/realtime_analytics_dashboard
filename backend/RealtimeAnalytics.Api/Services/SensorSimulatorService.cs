using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RealtimeAnalytics.Api.Data;
using System.Text.Json;

namespace RealtimeAnalytics.Api.Services
{
    public class SensorSimulatorService : BackgroundService
    {
        private readonly ReadingStore _store;
        private readonly StatsService _stats;
        private readonly WebSocketHub _hub;
        private readonly ILogger<SensorSimulatorService> _logger;
        private readonly int _sensors;
        private readonly int _rps;
        private readonly double _min;
        private readonly double _max;
        private readonly double _vol;
        private readonly Random _rng = new();

        public SensorSimulatorService(IConfiguration cfg, ReadingStore store, StatsService stats, WebSocketHub hub, ILogger<SensorSimulatorService> logger)
        {
            _store = store;
            _stats = stats;
            _hub = hub;
            _logger = logger;

            _sensors = cfg.GetSection("Simulation").GetValue<int>("Sensors", 100);
            _rps = cfg.GetSection("Simulation").GetValue<int>("ReadingsPerSecond", 1000);
            _min = cfg.GetSection("Simulation").GetValue<double>("ValueMin", 0.0);
            _max = cfg.GetSection("Simulation").GetValue<double>("ValueMax", 100.0);
            _vol = cfg.GetSection("Simulation").GetValue<double>("BaseVolatility", 2.5);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var sensorBases = Enumerable.Range(0, _sensors).Select(i => _min + (_max - _min) * i / Math.Max(1, _sensors - 1)).ToArray();
            var batchDelay = TimeSpan.FromMilliseconds(1000.0 / 20.0); // 20 batches/sec
            var batch = Math.Max(1, _rps / 20);

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTimeOffset.UtcNow;
                var batchItems = new List<object>();
                for (int i = 0; i < batch; i++)
                {
                    var sid = $"sensor-{_rng.Next(_sensors):D4}";
                    var baseVal = sensorBases[_rng.Next(_sensors)];
                    var value = Clamp(baseVal + (NextGaussian() * _vol), _min, _max);
                    if (_rng.NextDouble() < 0.002)
                        value = Clamp(value + (_rng.NextDouble() * 50 - 25), _min, _max);

                    var anomaly = _stats.IsAnomaly(value);
                    var reading = new Reading { SensorId = sid, Value = value, Timestamp = now, Anomaly = anomaly };
                    _store.Add(reading);
                    _stats.Ingest(value, now, anomaly);
                    batchItems.Add(new { type = "reading", sensorId = sid, value, timestamp = now, anomaly });
                }

                var payload = JsonSerializer.Serialize(new { type = "batch", items = batchItems });
                await _hub.BroadcastAsync(payload, stoppingToken);
                await Task.Delay(batchDelay, stoppingToken);
            }
        }

        private static double Clamp(double x, double lo, double hi) => Math.Min(hi, Math.Max(lo, x));

        private double NextGaussian()
        {
            var u1 = 1.0 - _rng.NextDouble();
            var u2 = 1.0 - _rng.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        }
    }
}