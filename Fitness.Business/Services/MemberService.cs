using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fitness.Business.DTOs;
using Fitness.Business.Interfaces;
using Fitness.Business.Models;
using Fitness.Business.Exceptions;
using Microsoft.Extensions.Logging;

namespace Fitness.Business.Services
{
    public class MemberService : IMemberService
    {
        private readonly IMemberRepository _memberRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly ILogger<MemberService> _logger;

        public MemberService(
            IMemberRepository memberRepository,
            IReservationRepository reservationRepository,
            ILogger<MemberService> logger)
        {
            _memberRepository = memberRepository;
            _reservationRepository = reservationRepository;
            _logger = logger;
        }

        public async Task<MemberDto> GetMemberAsync(int id)
        {
            var member = await _memberRepository.GetByIdAsync(id);
            if (member == null) throw new NotFoundException($"Member with ID {id} not found");
            
            return new MemberDto(
                member.MemberId,
                member.FirstName,
                member.LastName,
                member.Email ?? string.Empty,
                member.Address,
                member.Birthday,
                member.Interests,
                member.MemberType ?? "Bronze"
            );
        }

        public async Task<IEnumerable<MemberDto>> GetAllMembersAsync()
        {
            var members = await _memberRepository.GetAllAsync();
            return members.Select(m => new MemberDto(
                m.MemberId,
                m.FirstName,
                m.LastName,
                m.Email ?? string.Empty,
                m.Address,
                m.Birthday,
                m.Interests,
                m.MemberType ?? "Bronze"
            ));
        }

        public async Task CreateMemberAsync(CreateMemberDto memberDto)
        {
            // Basis validatie
            if (string.IsNullOrWhiteSpace(memberDto.FirstName))
                throw new ArgumentException("First name is required");
            if (string.IsNullOrWhiteSpace(memberDto.LastName))
                throw new ArgumentException("Last name is required");
            if (string.IsNullOrWhiteSpace(memberDto.Email))
                throw new ArgumentException("Email is required");
            if (string.IsNullOrWhiteSpace(memberDto.Address))
                throw new ArgumentException("Address is required");

            // Validatie email
            if (!IsValidEmail(memberDto.Email))
                throw new ArgumentException("Invalid email format");

            // Validatie geboortedatum
            if (memberDto.Birthday > DateTime.Now)
                throw new ArgumentException("Birthday cannot be in the future");

            var member = new Member
            {
                FirstName = memberDto.FirstName,
                LastName = memberDto.LastName,
                Email = memberDto.Email,
                Address = memberDto.Address,
                Birthday = memberDto.Birthday,
                Interests = memberDto.Interests,
                MemberType = "Bronze"
            };

            await _memberRepository.AddAsync(member);
            await _memberRepository.SaveChangesAsync();
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public async Task<MemberDto> UpdateMemberAsync(UpdateMemberDto dto)
        {
            if (dto == null)
                throw new ValidationException("Member data is required");

            var member = await _memberRepository.GetByIdAsync(dto.MemberId);
            if (member == null)
                throw new NotFoundException($"Member {dto.MemberId} not found");

            member.FirstName = dto.FirstName;
            member.LastName = dto.LastName;
            member.Email = dto.Email;
            member.Address = dto.Address;
            //Birthday en MemberType mogen niet aangepast worden na creatie

            await _memberRepository.UpdateAsync(member);
            await _memberRepository.SaveChangesAsync();

            return new MemberDto(
                member.MemberId,
                member.FirstName,
                member.LastName,
                member.Email ?? string.Empty,
                member.Address,
                member.Birthday,
                member.Interests,
                member.MemberType ?? "Bronze" 
            );
        }

        public async Task DeleteMemberAsync(int memberId)
        {
            var member = await _memberRepository.GetByIdAsync(memberId);
            if (member == null) throw new NotFoundException($"Member with ID {memberId} not found");

            await _memberRepository.DeleteAsync(member);
            await _memberRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<ReservationDto>> GetMemberReservationsAsync(int memberId)
        {
            var reservations = await _reservationRepository.GetByMemberAsync(memberId);
            return reservations.Select(r => new ReservationDto(
                r.ReservationId,
                r.Date,
                r.Equipment.DeviceType,
                string.Join(", ", r.TimeSlots.Select(ts => $"{ts.StartTime}:00 - {ts.EndTime}:00"))
            ));
        }

        public async Task<IEnumerable<TrainingSessionDto>> GetMemberTrainingSessionsAsync(int memberId, DateTime? fromDate = null)
        {
            var member = await _memberRepository.GetByIdAsync(memberId);
            if (member == null) throw new NotFoundException($"Member with ID {memberId} not found");

            var trainingSessions = new List<TrainingSessionDto>();

            // cycling sessions
            var cyclingSessions = await _memberRepository.GetCyclingSessionsAsync(memberId);
            trainingSessions.AddRange(cyclingSessions
                .Where(cs => !fromDate.HasValue || cs.Date >= fromDate.Value)
                .Select(cs => new TrainingSessionDto(
                    cs.CyclingSessionId,
                    cs.Date,
                    cs.Duration,
                    "Cycling",
                    $"Avg Watt: {cs.AvgWatt}, Max Watt: {cs.MaxWatt}"
                )));

            // running sessions
            var runningSessions = await _memberRepository.GetRunningSessionsAsync(memberId);
            trainingSessions.AddRange(runningSessions
                .Where(rs => !fromDate.HasValue || rs.Date >= fromDate.Value)
                .Select(rs => new TrainingSessionDto(
                    rs.RunningSessionId,
                    rs.Date,
                    rs.Duration,
                    "Running",
                    $"Avg Speed: {rs.AvgSpeed} km/h"
                )));

            return trainingSessions.OrderByDescending(ts => ts.Date);
        }

        public async Task<IEnumerable<TimeSlot>> GetMemberReservationTimeSlotsAsync(int memberId)
        {
            var reservations = await _reservationRepository.GetByMemberAsync(memberId);
            return reservations.SelectMany(r => r.TimeSlots).Distinct();
        }
    }
}
