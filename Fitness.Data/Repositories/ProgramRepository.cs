using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Fitness.Business.Interfaces;
using Fitness.Business.Models;
using Fitness.Data.Context;

namespace Fitness.Data.Repositories
{
    public class ProgramRepository : GenericRepository<FitnessProgram>, IProgramRepository
    {
        public ProgramRepository(FitnessContext context) : base(context) { }

        public async Task<FitnessProgram?> GetByProgramCodeAsync(string programCode)
        {
            return await _dbSet
                .Include(p => p.ProgramMembers)
                .FirstOrDefaultAsync(p => p.ProgramCode == programCode);
        }

        public async Task<int> GetCurrentMemberCountAsync(string programCode)
        {
            return await _context.ProgramMembers
                .CountAsync(pm => pm.ProgramCode == programCode);
        }

        public async Task AddMemberToProgramAsync(string programCode, int memberId)
        {
            var programMember = new ProgramMembers
            {
                ProgramCode = programCode,
                MemberId = memberId
            };

            await _context.ProgramMembers.AddAsync(programMember);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Member>> GetProgramMembersAsync(string programCode)
        {
            return await _context.ProgramMembers
                .Where(pm => pm.ProgramCode == programCode)
                .Include(pm => pm.Member)
                .Select(pm => pm.Member)
                .ToListAsync();
        }

        public async Task<IEnumerable<FitnessProgram>> GetActiveAsync(DateTime date)
        {
            return await _dbSet
                .Where(p => p.StartDate >= date)
                .Include(p => p.ProgramMembers)
                .ToListAsync();
        }

        public async Task<bool> IsMemberEnrolledAsync(string programCode, int memberId)
        {
            return await _context.ProgramMembers
                .AnyAsync(pm => pm.ProgramCode == programCode && pm.MemberId == memberId);
        }

        public override async Task<IEnumerable<FitnessProgram>> GetAllAsync()
        {
            return await _dbSet
                .Include(p => p.ProgramMembers)
                .ToListAsync();
        }
    }
}
