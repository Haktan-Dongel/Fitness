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

        public RunningSessionService(
            IRunningSessionRepository repository,
            ILogger<RunningSessionService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<RunningSessionMain>> GetAllForMemberAsync(int memberId)
        {
            _logger.LogDebug("Fetching all running sessions for member {MemberId}", memberId);
            return await _repository.GetByMemberAsync(memberId);
        }

        public async Task<IEnumerable<RunningSessionMain>> GetByMonthAsync(int memberId, int year, int month)
        {
            _logger.LogDebug("Fetching running sessions for member {MemberId} for {Year}-{Month}", 
                memberId, year, month);
            return await _repository.GetByMonthAsync(memberId, year, month);
        }

        public async Task<IEnumerable<RunningSessionMain>> GetByDateRangeAsync(
            int memberId, DateTime startDate, DateTime endDate)
        {
            _logger.LogDebug("Fetching running sessions for member {MemberId} between {StartDate} and {EndDate}",
                memberId, startDate, endDate);
            return await _repository.GetByDateRangeAsync(memberId, startDate, endDate);
        }

        public async Task<(double TotalDistance, double AverageSpeed)> GetPerformanceStatsAsync(
            int memberId, DateTime startDate, DateTime endDate)
        {
            _logger.LogDebug("Calculating performance stats for member {MemberId} between {StartDate} and {EndDate}",
                memberId, startDate, endDate);
                
            return await _repository.GetPerformanceStatsAsync(memberId, startDate, endDate);
        }
    }
}
