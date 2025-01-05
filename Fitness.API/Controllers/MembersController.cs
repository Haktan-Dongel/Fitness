using Microsoft.AspNetCore.Mvc;
using Fitness.Business.Services;
using Fitness.Business.DTOs;
using Fitness.Business.Exceptions;

namespace Fitness.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembersController : ControllerBase
    {
        private readonly IMemberService _memberService;
        private readonly ILogger<MembersController> _logger;

        public MembersController(IMemberService memberService, ILogger<MembersController> logger)
        {
            _memberService = memberService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetMembers()
        {
            try
            {
                var members = await _memberService.GetAllMembersAsync();
                return Ok(members);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all members");
                return StatusCode(500, "Error retrieving members");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MemberDto>> GetMember(int id)
        {
            try
            {
                var member = await _memberService.GetMemberAsync(id);
                return Ok(member);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting member {MemberId}", id);
                return StatusCode(500, "Error retrieving member");
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateMember(CreateMemberDto memberDto)
        {
            try
            {
                await _memberService.CreateMemberAsync(memberDto);
                return Ok("Member created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating member");
                return StatusCode(500, "Error creating member");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MemberDto>> UpdateMember(int id, [FromBody] UpdateMemberDto memberDto)
        {
            try
            {
                if (id != memberDto.MemberId)
                    return BadRequest("ID mismatch");

                await _memberService.UpdateMemberAsync(memberDto);

                return Ok("Member updated successfully");
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating member {MemberId}", id);
                return StatusCode(500, "Error updating member");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMember(int id)
        {
            try
            {
                await _memberService.DeleteMemberAsync(id);
                return Ok("Member deleted successfully");
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting member {MemberId}", id);
                return StatusCode(500, "Error deleting member");
            }
        }

        [HttpGet("{id}/reservations")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetMemberReservations(int id)
        {
            try
            {
                var reservations = await _memberService.GetMemberReservationsAsync(id);
                return Ok(reservations);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reservations for member {MemberId}", id);
                return StatusCode(500, "Error retrieving member reservations");
            }
        }

        [HttpGet("{id}/training-sessions")]
        public async Task<ActionResult<IEnumerable<TrainingSessionDto>>> GetMemberTrainingSessions(
            int id, 
            [FromQuery] DateTime? fromDate)
        {
            try
            {
                var sessions = await _memberService.GetMemberTrainingSessionsAsync(id, fromDate);
                return Ok(sessions);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting training sessions for member {MemberId}", id);
                return StatusCode(500, "Error retrieving member training sessions");
            }
        }
    }
}
