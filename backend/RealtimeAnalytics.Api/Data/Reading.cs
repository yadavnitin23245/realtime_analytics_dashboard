using System;

namespace RealtimeAnalytics.Api.Data
{
    public class Reading
    {
        public string SensorId { get; set; } = string.Empty;
        public double Value { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public bool Anomaly { get; set; }
    }
}