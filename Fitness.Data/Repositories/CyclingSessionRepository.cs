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
    }
}
