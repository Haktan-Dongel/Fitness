using Microsoft.AspNetCore.Mvc;
using Fitness.Business.Interfaces;
using Fitness.Business.Models;
using Fitness.Business.DTOs;

namespace Fitness.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RunningSessionsController : ControllerBase
    {
        private readonly IRunningSessionRepository _repository;
        private readonly ILogger<RunningSessionsController> _logger;

        public RunningSessionsController(IRunningSessionRepository repository, ILogger<RunningSessionsController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RunningSessionMain>>> GetAll()
        {
            try
            {
                var sessions = await _repository.GetAllAsync();
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving running sessions");
                return StatusCode(500, "Error retrieving running sessions");
            }
        }

        [HttpGet("{sessionId}")]
        public async Task<ActionResult<RunningSessionMain>> GetWithDetails(int sessionId)
        {
            try
            {
                var session = await _repository.GetWithDetailsAsync(sessionId);
                if (session == null)
                    return NotFound();

                return Ok(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving running session {SessionId}", sessionId);
                return StatusCode(500, "Error retrieving running session");
            }
        }

        [HttpGet("member/{memberId}")]
        public async Task<ActionResult<IEnumerable<RunningSessionMain>>> GetByMember(
            int memberId, 
            [FromQuery] DateTime? fromDate)
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
                _logger.LogError(ex, "Error retrieving running sessions for member {MemberId}", memberId);
                return StatusCode(500, "Error retrieving running sessions");
            }
        }

        [HttpPost]
        public async Task<ActionResult<RunningSessionMain>> Create([FromBody] CreateRunningSessionDto dto)
        {
            try
            {
                var session = new RunningSessionMain
                {
                    Date = dto.Date,
                    MemberId = dto.MemberId,
                    Duration = dto.Duration,
                    AvgSpeed = (int)dto.AvgSpeed
                };

                await _repository.AddAsync(session);
                await _repository.SaveChangesAsync();

                var details = dto.Intervals.Select(i => new RunningSessionDetail
                {
                    RunningSessionId = session.RunningSessionId,
                    SeqNr = i.SeqNr,
                    IntervalTime = i.IntervalTime,
                    IntervalSpeed = i.IntervalSpeed
                }).ToList();

                await _repository.AddDetailsAsync(session.RunningSessionId, details);
                
                return CreatedAtAction(nameof(GetWithDetails), 
                    new { sessionId = session.RunningSessionId }, session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating running session");
                return StatusCode(500, "Error creating running session");
            }
        }
    }
}
