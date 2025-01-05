using Microsoft.AspNetCore.Mvc;
using Fitness.Business.Interfaces;
using Fitness.Business.Models;

namespace Fitness.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CyclingSessionsController : ControllerBase
    {
        private readonly ICyclingSessionRepository _repository;
        private readonly ILogger<CyclingSessionsController> _logger;

        public CyclingSessionsController(ICyclingSessionRepository repository, ILogger<CyclingSessionsController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CyclingSession>>> GetAll()
        {
            try
            {
                var sessions = await _repository.GetAllAsync();
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cycling sessions");
                return StatusCode(500, "Error retrieving cycling sessions");
            }
        }

        [HttpGet("member/{memberId}")]
        public async Task<ActionResult<IEnumerable<CyclingSession>>> GetByMember(int memberId, [FromQuery] DateTime? fromDate)
        {
            try
            {
                var sessions = await _repository.GetByMemberAsync(memberId);
                if (fromDate.HasValue)
                {
                    sessions = sessions.Where(s => s.Date >= fromDate.Value);
                }
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cycling sessions for member {MemberId}", memberId);
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
                var stats = await _repository.GetPerformanceStatsAsync(memberId, startDate, endDate);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cycling stats for member {MemberId}", memberId);
                return StatusCode(500, "Error retrieving cycling stats");
            }
        }
    }
}
