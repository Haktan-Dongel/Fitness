using Microsoft.AspNetCore.Mvc;
using Fitness.Business.Interfaces;
using Fitness.Business.Models;

namespace Fitness.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProgramsController : ControllerBase
    {
        private readonly IProgramRepository _programRepository;
        private readonly ILogger<ProgramsController> _logger;

        public ProgramsController(IProgramRepository programRepository, ILogger<ProgramsController> logger)
        {
            _programRepository = programRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FitnessProgram>>> GetAllPrograms()
        {
            try
            {
                var programs = await _programRepository.GetAllAsync();
                return Ok(programs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving programs");
                return StatusCode(500, "Error retrieving programs");
            }
        }

        [HttpGet("{programCode}/members")]
        public async Task<ActionResult<IEnumerable<Member>>> GetProgramMembers(string programCode)
        {
            try
            {
                var members = await _programRepository.GetProgramMembersAsync(programCode);
                return Ok(members);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving program members");
                return StatusCode(500, "Error retrieving program members");
            }
        }

        [HttpPost("{programCode}/members/{memberId}")]
        public async Task<ActionResult> AddMemberToProgram(string programCode, int memberId)
        {
            try
            {
                if (!await _programRepository.HasAvailableSpaceAsync(programCode))
                {
                    return BadRequest("Program is full");
                }

                await _programRepository.AddMemberToProgramAsync(programCode, memberId);
                return Ok("Member added to program successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding member to program");
                return StatusCode(500, "Error adding member to program");
            }
        }

        [HttpGet("{programCode}/has-space")]
        public async Task<ActionResult<bool>> CheckAvailableSpace(string programCode)
        {
            try
            {
                var hasSpace = await _programRepository.HasAvailableSpaceAsync(programCode);
                return Ok(hasSpace);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking program capacity");
                return StatusCode(500, "Error checking program capacity");
            }
        }
    }
}
