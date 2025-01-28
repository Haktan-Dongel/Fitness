using Microsoft.AspNetCore.Mvc;
using Fitness.Business.Interfaces;
using Fitness.Business.Models;
using Fitness.Business.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Fitness.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProgramsController : ControllerBase
    {
        private readonly IProgramRepository _programRepository;
        private readonly IProgramService _programService;
        private readonly ILogger<ProgramsController> _logger;

        public ProgramsController(
            IProgramRepository programRepository, 
            IProgramService programService,
            ILogger<ProgramsController> logger)
        {
            _programRepository = programRepository;
            _programService = programService;
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
                await _programService.AddMemberToProgramAsync(programCode, memberId);
                return Ok("Member added to program successfully");
            }
            catch (Fitness.Business.Exceptions.ValidationException ex)
            {
                return BadRequest(ex.Message);
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
                var hasSpace = await _programService.HasAvailableSpaceAsync(programCode);
                return Ok(hasSpace);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking program capacity");
                return StatusCode(500, "Error checking program capacity");
            }
        }
    }
}
