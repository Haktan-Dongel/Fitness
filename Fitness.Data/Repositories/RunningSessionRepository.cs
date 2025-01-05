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
    public class RunningSessionRepository : GenericRepository<RunningSessionMain>, IRunningSessionRepository
    {
        private readonly ILogger<RunningSessionRepository> _logger;

        public RunningSessionRepository(FitnessContext context, ILogger<RunningSessionRepository> logger) : base(context)
        {
            _logger = logger;
        }

        public async Task<RunningSessionMain> GetWithDetailsAsync(int runningSessionId)
        {
            var session = await _dbSet
                .Include(rs => rs.Details)
                .FirstOrDefaultAsync(rs => rs.RunningSessionId == runningSessionId);
            
            if (session == null)
                throw new KeyNotFoundException($"Running session with ID {runningSessionId} was not found.");
                
            return session;
        }

        public async Task<IEnumerable<RunningSessionMain>> GetByMemberAsync(int memberId)
        {
            return await _dbSet
                .Where(rs => rs.MemberId == memberId)
                .Include(rs => rs.Details)
                .OrderByDescending(rs => rs.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<RunningSessionMain>> GetByMonthAsync(int memberId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            _logger.LogDebug("Fetching running sessions for member {MemberId} between {StartDate} and {EndDate}",
                memberId, startDate, endDate);

            var sessions = await _dbSet
                .Where(rs => rs.MemberId == memberId && 
                            rs.Date >= startDate && 
                            rs.Date <= endDate)
                .Include(rs => rs.Details)
                .OrderByDescending(rs => rs.Date)
                .ToListAsync();

            _logger.LogDebug("Found {Count} running sessions", sessions.Count);
            return sessions;
        }

        public async Task<IEnumerable<RunningSessionMain>> GetByDateRangeAsync(int memberId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(rs => rs.MemberId == memberId && 
                            rs.Date >= startDate && 
                            rs.Date <= endDate)
                .Include(rs => rs.Details)
                .OrderByDescending(rs => rs.Date)
                .ToListAsync();
        }

        public async Task AddDetailsAsync(int runningSessionId, IEnumerable<RunningSessionDetail> details)
        {
            foreach (var detail in details)
            {
                detail.RunningSessionId = runningSessionId;
                await _context.RunningSessionDetails.AddAsync(detail);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<(double TotalDistance, double AverageSpeed)> GetPerformanceStatsAsync(int memberId, DateTime startDate, DateTime endDate)
        {
            var sessions = await _dbSet
                .Where(rs => rs.MemberId == memberId && 
                            rs.Date >= startDate && 
                            rs.Date <= endDate)
                .ToListAsync();

            if (!sessions.Any())
                return (0, 0);

            var totalDistance = sessions.Sum(rs => (double)rs.Duration * (double)rs.AvgSpeed / 60.0);
            var avgSpeed = (double)sessions.Average(rs => rs.AvgSpeed);

            return (totalDistance, avgSpeed);
        }

        public override async Task<IEnumerable<RunningSessionMain>> GetAllAsync()
        {
            return await _dbSet
                .Include(rs => rs.Details)
                .OrderByDescending(rs => rs.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<RunningSessionMain>> GetAllForMemberAsync(int memberId)
        {
            return await _dbSet
                .Where(rs => rs.MemberId == memberId)
                .Include(rs => rs.Details)
                .OrderByDescending(rs => rs.Date)
                .ToListAsync();
        }
    }
}
