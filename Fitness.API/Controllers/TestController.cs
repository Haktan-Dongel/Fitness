using Microsoft.AspNetCore.Mvc;
using Fitness.Data.Context;
using Microsoft.EntityFrameworkCore;
using Fitness.Business.Interfaces;

namespace Fitness.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly FitnessContext _context;
        private readonly ILogger<TestController> _logger;
        private readonly IMemberRepository _memberRepository;

        public TestController(
            FitnessContext context, 
            ILogger<TestController> logger,
            IMemberRepository memberRepository)
        {
            _context = context;
            _logger = logger;
            _memberRepository = memberRepository;
        }

        [HttpGet("connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                await _context.Database.CanConnectAsync();
                
                var stats = new
                {
                    totalMembers = await _context.Members.CountAsync(),
                    activeReservations = await _context.Reservations
                        .Where(r => r.Date >= DateTime.Today)
                        .CountAsync(),
                    tableStats = new
                    {
                        Members = await _context.Members.CountAsync(),
                        Equipment = await _context.Equipment.CountAsync(),
                        Programs = await _context.Programs.CountAsync(),
                        Reservations = await _context.Reservations
                            .Where(r => r.Date >= DateTime.Today)
                            .CountAsync(),
                    }
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection test failed");
                return Problem(ex.Message);
            }
        }

        [HttpGet("members")]
        public async Task<IActionResult> GetMembers()
        {
            try
            {
                var members = await _memberRepository.GetAllAsync();
                return Ok(members);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get members");
                return Problem(ex.Message);
            }
        }
    }
}
