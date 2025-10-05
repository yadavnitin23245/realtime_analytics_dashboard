using Microsoft.AspNetCore.Mvc;
using RealtimeAnalytics.Api.Data;
using System.Linq;

namespace RealtimeAnalytics.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HistoryController : ControllerBase
    {
        private readonly ReadingStore _store;

        public HistoryController(ReadingStore store) => _store = store;

        [HttpGet]
        public IActionResult Get([FromQuery] int? hours)
        {
            var lookback = hours is null ? 1 : Math.Max(0, Math.Min(24, hours.Value));
            var since = DateTimeOffset.UtcNow.AddHours(-lookback);
            var items = _store.GetSince(since).Select(r => new { r.SensorId, r.Value, r.Timestamp, r.Anomaly });
            return Ok(items);
        }
    }
}