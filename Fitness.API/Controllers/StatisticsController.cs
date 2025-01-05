using Microsoft.AspNetCore.Mvc;
using Fitness.Business.DTOs;
using Fitness.Business.Interfaces;

namespace Fitness.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly ICyclingSessionRepository _cyclingRepo;
        private readonly IRunningSessionRepository _runningRepo;
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(
            ICyclingSessionRepository cyclingRepo,
            IRunningSessionRepository runningRepo,
            ILogger<StatisticsController> logger)
        {
            _cyclingRepo = cyclingRepo;
            _runningRepo = runningRepo;
            _logger = logger;
        }

        [HttpGet("member/{memberId}/monthly")]
        public async Task<ActionResult<MonthlySessionsDto>> GetMonthlyStats(
            int memberId,
            [FromQuery] int? year = null,
            [FromQuery] int? month = null)
        {
            try
            {
                var today = DateTime.Today;
                year ??= today.Year;
                month ??= today.Month;

                if (year < 2000 || year > 2100 || month < 1 || month > 12)
                {
                    return BadRequest("Invalid year or month parameters");
                }

                var startDate = new DateTime(year.Value, month.Value, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                _logger.LogInformation(
                    "Retrieving monthly stats for Member {MemberId} from {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}",
                    memberId, startDate, endDate);

                var cyclingSessions = await _cyclingRepo.GetByMonthAsync(memberId, year.Value, month.Value);
                var runningSessions = await _runningRepo.GetByMonthAsync(memberId, year.Value, month.Value);

                _logger.LogInformation("Found {CyclingCount} cycling sessions and {RunningCount} running sessions",
                    cyclingSessions.Count(), runningSessions.Count());

                var sessions = new List<SessionSummaryDto>();
                int totalDurationMinutes = 0;

                foreach (var cs in cyclingSessions)
                {
                    totalDurationMinutes += cs.Duration;
                    sessions.Add(new SessionSummaryDto(
                        cs.Date,
                        "Cycling",
                        cs.Duration,
                        $"Avg: {cs.AvgWatt}W, Max: {cs.MaxWatt}W",
                        CalculateTrainingImpact(cs.AvgWatt, cs.Duration)
                    ));
                }

                foreach (var rs in runningSessions)
                {
                    totalDurationMinutes += rs.Duration;
                    sessions.Add(new SessionSummaryDto(
                        rs.Date,
                        "Running",
                        rs.Duration,
                        $"Avg Speed: {rs.AvgSpeed} km/h",
                        CalculateRunningImpact(rs.AvgSpeed, rs.Duration)
                    ));
                }

                var sessionCount = sessions.Count;
                var averageDurationMinutes = sessionCount > 0 ? totalDurationMinutes / sessionCount : 0;
                var totalHours = Math.Round(totalDurationMinutes / 60.0, 2);

                var sessionsPerType = sessions
                    .GroupBy(s => s.Type)
                    .ToDictionary(g => g.Key, g => g.Count());

                var result = new MonthlySessionsDto(
                    year.Value,
                    month.Value,
                    sessionCount,
                    totalHours,
                    averageDurationMinutes,
                    sessionsPerType,
                    sessions.OrderByDescending(s => s.Date)
                );

                _logger.LogInformation(
                    "Monthly stats: Year={Year}, Month={Month}, Sessions={Count}, Hours={Hours}, AvgDuration={AvgDuration}",
                    result.Year, result.Month, result.TotalSessions, result.TotalHours, result.AverageDuration);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving monthly statistics for member {MemberId}", memberId);
                return StatusCode(500, "Error retrieving monthly statistics");
            }
        }

        [HttpGet("member/{memberId}/overview")]
        public async Task<ActionResult<TrainingStatisticsDto>> GetTrainingOverview(
            int memberId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var cyclingSessions = await _cyclingRepo.GetByDateRangeAsync(memberId, startDate, endDate);
                var runningSessions = await _runningRepo.GetByDateRangeAsync(memberId, startDate, endDate);

                var allDurations = new List<int>();
                allDurations.AddRange(cyclingSessions.Select(cs => cs.Duration));
                allDurations.AddRange(runningSessions.Select(rs => rs.Duration));

                if (!allDurations.Any())
                    return Ok(new TrainingStatisticsDto(0, 0, 0, 0, 0, new Dictionary<string, int>()));

                var sessionsByType = new Dictionary<string, int>
                {
                    { "Cycling", cyclingSessions.Count() },
                    { "Running", runningSessions.Count() }
                };

                return Ok(new TrainingStatisticsDto(
                    allDurations.Count,
                    allDurations.Sum() / 60.0,
                    allDurations.Min(),
                    allDurations.Max(),
                    allDurations.Average(),
                    sessionsByType
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving training overview for member {MemberId}", memberId);
                return StatusCode(500, "Error retrieving training overview");
            }
        }

        [HttpGet("member/{memberId}/yearly-summary")]
        public async Task<ActionResult<Dictionary<int, Dictionary<int, int>>>> GetYearlySummary(int memberId)
        {
            try
            {
                _logger.LogInformation("Fetching yearly summary for member {MemberId}", memberId);
                
                var cyclingSessions = await _cyclingRepo.GetAllForMemberAsync(memberId);
                var runningSessions = await _runningRepo.GetAllForMemberAsync(memberId);

                _logger.LogDebug("Found {CyclingCount} cycling sessions and {RunningCount} running sessions",
                    cyclingSessions.Count(), runningSessions.Count());
                    
                var cyclingDates = cyclingSessions.Select(cs => new { Date = cs.Date });
                var runningDates = runningSessions.Select(rs => new { Date = rs.Date });
                var allSessions = cyclingDates.Concat(runningDates);

                var summary = allSessions
                    .GroupBy(s => s.Date.Year)
                    .ToDictionary(
                        g => g.Key,
                        g => g.GroupBy(s => s.Date.Month)
                              .ToDictionary(mg => mg.Key, mg => mg.Count())
                    );

                _logger.LogInformation("Returning summary with {YearCount} years of data", summary.Count);
                
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving yearly summary for member {MemberId}", memberId);
                return StatusCode(500, "Error retrieving yearly summary");
            }
        }

        private string CalculateTrainingImpact(int avgWattage, int durationMinutes)
        {
            if (avgWattage < 150)
            {
                return durationMinutes <= 90 ? "low" : "medium";
            }
            
            if (avgWattage <= 200)
            {
                return "medium";
            }

            return "high";
        }

        private string CalculateRunningImpact(double avgSpeed, int durationMinutes)
        {
            if (avgSpeed < 8)
                return "low";
            if (avgSpeed <= 12)
                return "medium";
            return "high";
        }
    }
}