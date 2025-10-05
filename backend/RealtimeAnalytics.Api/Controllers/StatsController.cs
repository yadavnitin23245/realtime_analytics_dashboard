using Microsoft.AspNetCore.Mvc;
using RealtimeAnalytics.Api.Data;

namespace RealtimeAnalytics.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly StatsService _stats;

        public StatsController(StatsService stats) => _stats = stats;

        [HttpGet]
        public ActionResult<StatsSnapshot> Get() => Ok(_stats.GetSnapshot());
    }
}