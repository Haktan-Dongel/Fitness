using Fitness.Business.Interfaces;
using Fitness.Business.Models;
using Microsoft.Extensions.Logging;

namespace Fitness.Business.Services
{
    public class CyclingSessionService : ICyclingSessionService
    {
        private readonly ICyclingSessionRepository _repository;
        private readonly ILogger<CyclingSessionService> _logger;

        public CyclingSessionService(ICyclingSessionRepository repository, ILogger<CyclingSessionService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<CyclingSession>> GetByMonthAsync(int memberId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            _logger.LogDebug("Fetching cycling sessions for member {MemberId} between {StartDate} and {EndDate}",
                memberId, startDate, endDate);

            var sessions = await _repository.GetByDateRangeAsync(memberId, startDate, endDate);
            _logger.LogDebug("Found {Count} cycling sessions", sessions.Count());
            
            return sessions;
        }

        public async Task<(double AverageWatt, double MaxWatt)> GetPerformanceStatsAsync(int memberId, DateTime startDate, DateTime endDate)
        {
            var sessions = await _repository.GetByDateRangeAsync(memberId, startDate, endDate);

            if (!sessions.Any())
                return (0, 0);

            return (
                AverageWatt: sessions.Average(cs => (double)cs.AvgWatt),
                MaxWatt: sessions.Max(cs => (double)cs.MaxWatt)
            );
        }

        public async Task<IEnumerable<CyclingSession>> GetAllForMemberAsync(int memberId)
        {
            return await _repository.GetByMemberAsync(memberId);
        }

        public async Task<IEnumerable<CyclingSession>> GetByDateRangeAsync(int memberId, DateTime startDate, DateTime endDate)
        {
            _logger.LogDebug("Fetching cycling sessions for member {MemberId} between {StartDate} and {EndDate}",
                memberId, startDate, endDate);

            return await _repository.GetByDateRangeAsync(memberId, startDate, endDate);
        }
    }
}
