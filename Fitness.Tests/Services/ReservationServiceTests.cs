using Xunit;
using Moq;
using Fitness.Business.Models;
using Fitness.Business.Services;
using Fitness.Business.Interfaces;
using Fitness.Business.DTOs;
using Fitness.Business.Exceptions;
using Microsoft.Extensions.Logging;

public class ReservationServiceTests
{
    private readonly Mock<IReservationRepository> _mockReservationRepo;
    private readonly Mock<ITimeSlotRepository> _mockTimeSlotRepo;
    private readonly Mock<IEquipmentRepository> _mockEquipmentRepo;
    private readonly Mock<ILogger<ReservationService>> _mockLogger;
    private readonly ReservationService _service;

    public ReservationServiceTests()
    {
        _mockReservationRepo = new Mock<IReservationRepository>();
        _mockTimeSlotRepo = new Mock<ITimeSlotRepository>();
        _mockEquipmentRepo = new Mock<IEquipmentRepository>();
        _mockLogger = new Mock<ILogger<ReservationService>>();
        _service = new ReservationService(
            _mockReservationRepo.Object,
            _mockTimeSlotRepo.Object,
            _mockEquipmentRepo.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateReservationsAsync_WithValidData_CreatesReservation()
    {
        // Arrange
        var dto = new CreateReservationDto 
        { 
            MemberId = 1,
            EquipmentId = 1,
            Date = DateTime.Today,
            TimeSlotIds = new List<int> { 1 }
        };

        var timeSlot = new TimeSlot { TimeSlotId = 1, StartTime = 800, EndTime = 900 };
        var equipment = new Equipment { EquipmentId = 1, DeviceType = "Treadmill" };

        _mockReservationRepo.Setup(r => r.GetDailyReservationCountAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(0);
        _mockTimeSlotRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(timeSlot);
        _mockEquipmentRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(equipment);
        _mockReservationRepo.Setup(r => r.AddAsync(It.IsAny<Reservation>()))
            .ReturnsAsync((Reservation r) => r);

        // Act
        var result = await _service.CreateReservationsAsync(dto);

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Single(resultList);
        _mockReservationRepo.Verify(r => r.AddAsync(It.Is<Reservation>(
            res => res.MemberId == dto.MemberId 
                && res.EquipmentId == dto.EquipmentId
                && res.TimeSlots.Count == 1)), 
            Times.Once);
    }

    [Fact]
    public async Task ValidateReservationAsync_WhenDailyLimitReached_ReturnsFalse()
    {
        // Arrange
        _mockReservationRepo.Setup(r => r.GetDailyReservationCountAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(4);

        // Act
        var result = await _service.ValidateReservationAsync(1, DateTime.Today, 1, 1);

        // Assert
        Assert.False(result);
    }
}
