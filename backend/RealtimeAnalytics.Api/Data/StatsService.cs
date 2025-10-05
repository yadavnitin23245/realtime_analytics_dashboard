using System;

namespace RealtimeAnalytics.Api.Data
{
    public class StatsSnapshot
    {
        public double Avg { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public double StdDev { get; set; }
        public long Count { get; set; }
        public long Anomalies { get; set; }
        public DateTimeOffset LastTimestamp { get; set; }
    }

    public class StatsService
    {
        private long _n = 0;
        private double _mean = 0.0;
        private double _m2 = 0.0;
        private double _min = double.PositiveInfinity;
        private double _max = double.NegativeInfinity;
        private long _anomalies = 0;
        private DateTimeOffset _lastTs = DateTimeOffset.MinValue;
        private readonly object _lock = new();

        public bool IsAnomaly(double value)
        {
            var std = StdDevUnsafe();
            if (_n < 50 || std == 0) return false;
            return Math.Abs(value - _mean) > 3 * std;
        }

        public void Ingest(double value, DateTimeOffset ts, bool anomaly)
        {
            lock (_lock)
            {
                _n++;
                var delta = value - _mean;
                _mean += delta / _n;
                var delta2 = value - _mean;
                _m2 += delta * delta2;
                if (value < _min) _min = value;
                if (value > _max) _max = value;
                if (anomaly) _anomalies++;
                _lastTs = ts;
            }
        }

        private double StdDevUnsafe()
        {
            if (_n < 2) return 0;
            return Math.Sqrt(_m2 / (_n - 1));
        }

        public StatsSnapshot GetSnapshot()
        {
            lock (_lock)
            {
                return new StatsSnapshot
                {
                    Avg = _mean,
                    Min = double.IsPositiveInfinity(_min) ? 0 : _min,
                    Max = double.IsNegativeInfinity(_max) ? 0 : _max,
                    StdDev = StdDevUnsafe(),
                    Count = _n,
                    Anomalies = _anomalies,
                    LastTimestamp = _lastTs
                };
            }
        }
    }
}