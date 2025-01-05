using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fitness.Business.DTOs;

namespace Fitness.Business.Services
{
    public interface IMemberService
    {
        Task<MemberDto> GetMemberAsync(int id);
        Task<IEnumerable<MemberDto>> GetAllMembersAsync();
        Task CreateMemberAsync(CreateMemberDto memberDto);
        Task<MemberDto> UpdateMemberAsync(UpdateMemberDto memberDto);
        Task DeleteMemberAsync(int memberId);

        Task<IEnumerable<ReservationDto>> GetMemberReservationsAsync(int memberId);
        Task<IEnumerable<TrainingSessionDto>> GetMemberTrainingSessionsAsync(int memberId, DateTime? fromDate = null);
    }
}
