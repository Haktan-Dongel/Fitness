using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Fitness.Business.Interfaces;
using Fitness.Business.Models;
using Fitness.Data.Context;
using Microsoft.Extensions.Logging;

namespace Fitness.Data.Repositories
{
    public class CyclingSessionRepository : GenericRepository<CyclingSession>, ICyclingSessionRepository
    {
        private readonly ILogger<CyclingSessionRepository> _logger;

        public CyclingSessionRepository(FitnessContext context, ILogger<CyclingSessionRepository> logger) : base(context) 
        {
            _logger = logger;
        }

        public async Task<IEnumerable<CyclingSession>> GetByMemberAsync(int memberId)
        {
            return await _dbSet
                .Where(cs => cs.MemberId == memberId)
                .OrderByDescending(cs => cs.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<CyclingSession>> GetByDateRangeAsync(int memberId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(cs => cs.MemberId == memberId 
                         && cs.Date >= startDate 
                         && cs.Date <= endDate)
                .OrderByDescending(cs => cs.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<CyclingSession>> GetByMonthAsync(int memberId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            _logger.LogDebug("Fetching cycling sessions for member {MemberId} between {StartDate} and {EndDate}",
                memberId, startDate, endDate);

            var sessions = await _dbSet
                .Where(cs => cs.MemberId == memberId && 
                            cs.Date >= startDate && 
                            cs.Date <= endDate)
                .OrderByDescending(cs => cs.Date)
                .ToListAsync();

            _logger.LogDebug("Found {Count} cycling sessions", sessions.Count);
            return sessions;
        }

        public async Task<(double TotalWatt, double MaxWatt)> GetPerformanceStatsAsync(int memberId, DateTime startDate, DateTime endDate)
        {
            var sessions = await _dbSet
                .Where(cs => cs.MemberId == memberId 
                         && cs.Date >= startDate 
                         && cs.Date <= endDate)
                .ToListAsync();

            if (!sessions.Any())
                return (0, 0);

            return (
                TotalWatt: sessions.Average(cs => (double)cs.AvgWatt),
                MaxWatt: sessions.Max(cs => (double)cs.MaxWatt)
            );
        }

        public async Task<IEnumerable<CyclingSession>> GetAllForMemberAsync(int memberId)
        {
            return await GetByMemberAsync(memberId);
        }
    }
}
