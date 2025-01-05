using Xunit;
using Moq;
using Fitness.Business.Services;
using Fitness.Business.Interfaces;
using Fitness.Business.Models;
using Fitness.Business.DTOs;
using Fitness.Business.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Fitness.Tests.Services
{
    public class MemberServiceTests
    {
        private readonly Mock<IMemberRepository> _mockRepository;
        private readonly MemberService _service;

        public MemberServiceTests()
        {
            _mockRepository = new Mock<IMemberRepository>();
            _service = new MemberService(_mockRepository.Object);
        }

        [Fact]
        public async Task GetMemberAsync_WithValidId_ReturnsMemberDto()
        {
            var memberId = 1;
            var member = new Member
            {
                MemberId = memberId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Address = "Test City",
                Birthday = new DateTime(1990, 1, 1),
                Interests = "Running",
                MemberType = "Silver"
            };

            _mockRepository.Setup(repo => repo.GetByIdAsync(memberId))
                .ReturnsAsync(member);

            var result = await _service.GetMemberAsync(memberId);

            Assert.NotNull(result);
            Assert.Equal(member.FirstName, result.FirstName);
            Assert.Equal(member.LastName, result.LastName);
            Assert.Equal(member.Email, result.Email);
        }

        [Fact]
        public async Task GetMemberAsync_WithInvalidId_ThrowsNotFoundException()
        {
            var memberId = 999;
            _mockRepository.Setup(repo => repo.GetByIdAsync(memberId))
                .ReturnsAsync((Member?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => 
                _service.GetMemberAsync(memberId));
        }

        [Fact]
        public async Task CreateMemberAsync_WithValidData_CreatesNewMember()
        {
            var createDto = new CreateMemberDto(
                "John",
                "Doe",
                "john@example.com",
                "Test City",
                new DateTime(1990, 1, 1),
                "Running"
            );

            Member? savedMember = null;
            _mockRepository.Setup(repo => repo.AddAsync(It.IsAny<Member>()))
                .Callback<Member>(member => savedMember = member)
                .Returns(Task.CompletedTask);

            await _service.CreateMemberAsync(createDto);

            _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Member>()), Times.Once);
            _mockRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
            Assert.NotNull(savedMember);
            Assert.Equal(createDto.FirstName, savedMember.FirstName);
            Assert.Equal(createDto.LastName, savedMember.LastName);
            Assert.Equal("Bronze", savedMember.MemberType);
        }

        [Fact]
        public async Task UpdateMemberAsync_WithValidData_UpdatesMember()
        {
            var memberId = 1;
            var updateDto = new UpdateMemberDto(
                memberId,
                "John",
                "Updated",
                "john@example.com",
                "New Address"
            );

            var existingMember = new Member
            {
                MemberId = memberId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Address = "Old Address",
                Birthday = new DateTime(1990, 1, 1),
                MemberType = "Silver"
            };

            _mockRepository.Setup(repo => repo.GetByIdAsync(memberId))
                .ReturnsAsync(existingMember);

            var result = await _service.UpdateMemberAsync(updateDto);

            Assert.Equal(updateDto.FirstName, result.FirstName);
            Assert.Equal(updateDto.LastName, result.LastName);
            Assert.Equal(updateDto.Address, result.Address);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Member>()), Times.Once);
        }

        [Fact]
        public async Task DeleteMemberAsync_WithValidId_DeletesMember()
        {
            var memberId = 1;
            var member = new Member { MemberId = memberId };
            _mockRepository.Setup(repo => repo.GetByIdAsync(memberId))
                .ReturnsAsync(member);

            await _service.DeleteMemberAsync(memberId);

            _mockRepository.Verify(repo => repo.DeleteAsync(member), Times.Once);
            _mockRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetMemberReservationsAsync_WithValidId_ReturnsReservations()
        {
            var memberId = 1;
            var member = new Member { MemberId = memberId };
            var reservations = new List<Reservation>
            {
                new Reservation 
                { 
                    ReservationId = 1,
                    Date = DateTime.Today,
                    Equipment = new Equipment { DeviceType = "Treadmill" },
                    TimeSlot = new TimeSlot { StartTime = 8, EndTime = 9 }
                }
            };

            _mockRepository.Setup(repo => repo.GetByIdAsync(memberId))
                .ReturnsAsync(member);
            _mockRepository.Setup(repo => repo.GetReservationsAsync(memberId))
                .ReturnsAsync(reservations);

            var result = await _service.GetMemberReservationsAsync(memberId);

            Assert.Single(result);
            var reservation = result.First();
            Assert.Equal("Treadmill", reservation.Equipment);
            Assert.Equal("8:00 - 9:00", reservation.TimeSlot);
        }

        [Fact]
        public async Task GetAllMembersAsync_ReturnsAllMembers()
        {
            var members = new List<Member>
            {
                new Member 
                { 
                    MemberId = 1, 
                    FirstName = "John", 
                    LastName = "Doe",
                    Email = "john@example.com",
                    Address = "City1",
                    Birthday = new DateTime(1990, 1, 1),
                    MemberType = "Silver"
                },
                new Member 
                { 
                    MemberId = 2, 
                    FirstName = "Jane", 
                    LastName = "Smith",
                    Email = "jane@example.com",
                    Address = "City2",
                    Birthday = new DateTime(1995, 1, 1),
                    MemberType = "Gold"
                }
            };

            _mockRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(members);

            var result = await _service.GetAllMembersAsync();

            var membersList = result.ToList();
            Assert.Equal(2, membersList.Count);
            Assert.Equal("John", membersList[0].FirstName);
            Assert.Equal("Jane", membersList[1].FirstName);
        }

        [Fact]
        public async Task GetMemberTrainingSessionsAsync_ReturnsTrainingSessions()
        {
            var memberId = 1;
            var member = new Member { MemberId = memberId };
            var cyclingSessions = new List<CyclingSession>
            {
                new CyclingSession 
                { 
                    CyclingSessionId = 1,
                    Date = DateTime.Today,
                    Duration = 60,
                    AvgWatt = 200,
                    MaxWatt = 300
                }
            };
            var runningSessions = new List<RunningSessionMain>
            {
                new RunningSessionMain
                {
                    RunningSessionId = 1,
                    Date = DateTime.Today,
                    Duration = 45,
                    AvgSpeed = 12.5
                }
            };

            _mockRepository.Setup(repo => repo.GetByIdAsync(memberId))
                .ReturnsAsync(member);
            _mockRepository.Setup(repo => repo.GetCyclingSessionsAsync(memberId))
                .ReturnsAsync(cyclingSessions);
            _mockRepository.Setup(repo => repo.GetRunningSessionsAsync(memberId))
                .ReturnsAsync(runningSessions);

            var result = await _service.GetMemberTrainingSessionsAsync(memberId);

            var sessions = result.ToList();
            Assert.Equal(2, sessions.Count);
            Assert.Contains(sessions, s => s.Type == "Cycling");
            Assert.Contains(sessions, s => s.Type == "Running");
        }

        [Fact]
        public async Task UpdateMemberAsync_WithInvalidId_ThrowsNotFoundException()
        {
            var updateDto = new UpdateMemberDto(
                999,
                "John",
                "Doe",
                "john@example.com",
                "Test City"
            );

            _mockRepository.Setup(repo => repo.GetByIdAsync(999))
                .ReturnsAsync((Member?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => 
                _service.UpdateMemberAsync(updateDto));
        }

        [Theory]
        [InlineData("", "Doe", "test@example.com", "City")]
        [InlineData("John", "", "test@example.com", "City")]
        [InlineData("John", "Doe", "", "City")]
        [InlineData("John", "Doe", "test@example.com", "")]
        public async Task CreateMemberAsync_WithInvalidData_ThrowsArgumentException(
            string firstName, string lastName, string email, string address)
        {
            var createDto = new CreateMemberDto(
                firstName,
                lastName,
                email,
                address,
                DateTime.Now,
                null
            );

            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateMemberAsync(createDto));
        }

        [Theory]
        [InlineData("notanemail")]
        [InlineData("@nodomain")]
        [InlineData("noat.com")]
        public async Task CreateMemberAsync_WithInvalidEmail_ThrowsArgumentException(string email)
        {
            var createDto = new CreateMemberDto(
                "John",
                "Doe",
                email,
                "City",
                DateTime.Now,
                null
            );

            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateMemberAsync(createDto));
        }

        [Fact]
        public async Task CreateMemberAsync_WithFutureBirthday_ThrowsArgumentException()
        {
            var createDto = new CreateMemberDto(
                "John",
                "Doe",
                "john@example.com",
                "City",
                DateTime.Now.AddDays(1),
                null
            );

            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateMemberAsync(createDto));
        }

        [Fact]
        public async Task GetMemberReservationsAsync_WithNoReservations_ReturnsEmptyList()
        {
            var memberId = 1;
            var member = new Member { MemberId = memberId };
            _mockRepository.Setup(repo => repo.GetByIdAsync(memberId))
                .ReturnsAsync(member);
            _mockRepository.Setup(repo => repo.GetReservationsAsync(memberId))
                .ReturnsAsync(new List<Reservation>());

            var result = await _service.GetMemberReservationsAsync(memberId);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMemberTrainingSessionsAsync_WithDateFilter_ReturnsFilteredSessions()
        {
            var memberId = 1;
            var member = new Member { MemberId = memberId };
            var filterDate = DateTime.Today.AddDays(-7);
            var cyclingSessions = new List<CyclingSession>
            {
                new CyclingSession 
                { 
                    Date = DateTime.Today,
                    Duration = 60
                },
                new CyclingSession 
                { 
                    Date = DateTime.Today.AddDays(-10),
                    Duration = 45
                }
            };

            _mockRepository.Setup(repo => repo.GetByIdAsync(memberId))
                .ReturnsAsync(member);
            _mockRepository.Setup(repo => repo.GetCyclingSessionsAsync(memberId))
                .ReturnsAsync(cyclingSessions);
            _mockRepository.Setup(repo => repo.GetRunningSessionsAsync(memberId))
                .ReturnsAsync(new List<RunningSessionMain>());

            var result = await _service.GetMemberTrainingSessionsAsync(memberId, filterDate);

            Assert.Single(result);
            Assert.All(result, session => Assert.True(session.Date >= filterDate));
        }

        [Fact]
        public async Task GetMemberTrainingSessionsAsync_WithNoSessions_ReturnsEmptyList()
        {
            var memberId = 1;
            var member = new Member { MemberId = memberId };

            _mockRepository.Setup(repo => repo.GetByIdAsync(memberId))
                .ReturnsAsync(member);
            _mockRepository.Setup(repo => repo.GetCyclingSessionsAsync(memberId))
                .ReturnsAsync(new List<CyclingSession>());
            _mockRepository.Setup(repo => repo.GetRunningSessionsAsync(memberId))
                .ReturnsAsync(new List<RunningSessionMain>());

            var result = await _service.GetMemberTrainingSessionsAsync(memberId);

            Assert.Empty(result);
        }
    }
}
