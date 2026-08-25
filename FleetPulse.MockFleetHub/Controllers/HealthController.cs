using Microsoft.AspNetCore.Mvc;

namespace FleetPulse.MockFleetHub.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {

        [HttpGet()]
        public async Task<IActionResult> Get()
        {
            return Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
            });
        }        
    }
}
