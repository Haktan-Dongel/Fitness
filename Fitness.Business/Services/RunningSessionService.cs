using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Fitness.Business.Interfaces;
using Fitness.Business.Models;
using Microsoft.Extensions.Logging;

namespace Fitness.Business.Services
{
    public class RunningSessionService : IRunningSessionService
    {
        private readonly IRunningSessionRepository _repository;
        private readonly ILogger<RunningSessionService> _logger;

        public RunningSessionService(IRunningSessionRepository repository, ILogger<RunningSessionService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<RunningSessionMain>> GetAllForMemberAsync(int memberId)
        {
            return await _repository.GetByMemberAsync(memberId);
        }

        public async Task<IEnumerable<RunningSessionMain>> GetByMonthAsync(int memberId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            _logger.LogDebug("Fetching running sessions for member {MemberId} between {StartDate} and {EndDate}",
                memberId, startDate, endDate);

            var sessions = await _repository.GetByDateRangeAsync(memberId, startDate, endDate);
            _logger.LogDebug("Found {Count} running sessions", sessions.Count());
            
            return sessions;
        }

        public async Task<IEnumerable<RunningSessionMain>> GetByDateRangeAsync(int memberId, DateTime startDate, DateTime endDate)
        {
            return await _repository.GetByDateRangeAsync(memberId, startDate, endDate);
        }
    }
}
