using Xunit;
using Moq;
using Fitness.Business.Models;
using Fitness.Business.Services;
using Fitness.Business.Interfaces;
using Microsoft.Extensions.Logging;

public class MemberServiceTests
{
    private readonly Mock<IMemberRepository> _mockRepository;
    private readonly Mock<IReservationRepository> _mockReservationRepository;
    private readonly Mock<ILogger<MemberService>> _mockLogger;
    private readonly MemberService _service;

    public MemberServiceTests()
    {
        _mockRepository = new Mock<IMemberRepository>();
        _mockReservationRepository = new Mock<IReservationRepository>();
        _mockLogger = new Mock<ILogger<MemberService>>();
        _service = new MemberService(_mockRepository.Object, _mockReservationRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetMemberReservationsAsync_WithValidId_ReturnsReservations()
    {
        // Arrange
        var memberId = 1;
        var expectedReservations = new List<Reservation>
        {
            new Reservation 
            { 
                ReservationId = 1,
                MemberId = memberId,
                Date = DateTime.Today,
                Equipment = new Equipment { DeviceType = "Treadmill" },
                TimeSlots = new List<TimeSlot> 
                { 
                    new TimeSlot { StartTime = 800, EndTime = 900 } 
                }
            }
        };

        _mockReservationRepository.Setup(r => r.GetByMemberAsync(memberId))
            .ReturnsAsync(expectedReservations);

        // Act
        var result = await _service.GetMemberReservationsAsync(memberId);

        // Assert
        var resultList = result.ToList();
        Assert.NotEmpty(resultList);
        Assert.Single(resultList);
        Assert.Equal(expectedReservations[0].ReservationId, resultList[0].ReservationId);
    }
}
