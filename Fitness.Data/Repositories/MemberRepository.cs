using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Fitness.Business.Interfaces;
using Fitness.Business.Models;
using Fitness.Data.Context;

namespace Fitness.Data.Repositories
{
    public class MemberRepository : GenericRepository<Member>, IMemberRepository
    {
        public MemberRepository(FitnessContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Reservation>> GetReservationsAsync(int memberId)
        {
            return await _context.Reservations
                .Include(r => r.Equipment)
                .Include(r => r.TimeSlots) // Updated to include TimeSlots collection
                .Where(r => r.MemberId == memberId)
                .ToListAsync();
        }

        public async Task<IEnumerable<FitnessProgram>> GetProgramsAsync(int memberId)
        {
            return await _context.ProgramMembers
                .Include(pm => pm.Program)
                .Where(pm => pm.MemberId == memberId)
                .Select(pm => pm.Program)
                .ToListAsync();
        }

        public async Task<IEnumerable<CyclingSession>> GetCyclingSessionsAsync(int memberId)
        {
            return await _context.CyclingSessions
                .Where(cs => cs.MemberId == memberId)
                .ToListAsync();
        }

        public async Task<IEnumerable<RunningSessionMain>> GetRunningSessionsAsync(int memberId)
        {
            return await _context.RunningSessionMains
                .Include(rs => rs.Details)
                .Where(rs => rs.MemberId == memberId)
                .ToListAsync();
        }
    }
}
