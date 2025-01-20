using Microsoft.AspNetCore.Mvc;
using Fitness.Business.Interfaces;
using Fitness.Business.Models;

namespace Fitness.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CyclingSessionsController : ControllerBase
    {
        private readonly ICyclingSessionService _cyclingService;
        private readonly ILogger<CyclingSessionsController> _logger;

        public CyclingSessionsController(ICyclingSessionService cyclingService, ILogger<CyclingSessionsController> logger)
        {
            _cyclingService = cyclingService;
            _logger = logger;
        }

        [HttpGet("member/{memberId}")]
        public async Task<ActionResult<IEnumerable<CyclingSession>>> GetByMember(int memberId)
        {
            try
            {
                var sessions = await _cyclingService.GetAllForMemberAsync(memberId);
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cycling sessions for member {MemberId}", memberId);
                return StatusCode(500, "Error retrieving cycling sessions");
            }
        }

        [HttpGet("member/{memberId}/month")]
        public async Task<ActionResult<IEnumerable<CyclingSession>>> GetByMonth(
            int memberId,
            [FromQuery] int year,
            [FromQuery] int month)
        {
            try
            {
                var sessions = await _cyclingService.GetByMonthAsync(memberId, year, month);
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving monthly cycling sessions for member {MemberId}", memberId);
                return StatusCode(500, "Error retrieving cycling sessions");
            }
        }

        [HttpGet("stats/{memberId}")]
        public async Task<ActionResult<object>> GetMemberStats(
            int memberId, 
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            try
            {
                var stats = await _cyclingService.GetPerformanceStatsAsync(memberId, startDate, endDate);
                return Ok(new
                {
                    AverageWatt = stats.AverageWatt,
                    MaxWatt = stats.MaxWatt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cycling stats for member {MemberId}", memberId);
                return StatusCode(500, "Error retrieving cycling stats");
            }
        }
    }
}
