using Xunit;
using Moq;
using Fitness.Business.Models;
using Fitness.Business.Services;
using Fitness.Business.Interfaces;
using Microsoft.Extensions.Logging;

public class TimeSlotServiceTests
{
    private readonly Mock<ITimeSlotRepository> _mockRepository;
    private readonly Mock<ILogger<TimeSlotService>> _mockLogger;
    private readonly TimeSlotService _service;

    public TimeSlotServiceTests()
    {
        _mockRepository = new Mock<ITimeSlotRepository>();
        _mockLogger = new Mock<ILogger<TimeSlotService>>();
        _service = new TimeSlotService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTimeSlots()
    {
        // Arrange
        var expectedTimeSlots = new List<TimeSlot>
        {
            new TimeSlot { TimeSlotId = 1, StartTime = 800, EndTime = 900, PartOfDay = "Morning" },
            new TimeSlot { TimeSlotId = 2, StartTime = 900, EndTime = 1000, PartOfDay = "Morning" }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(expectedTimeSlots);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(expectedTimeSlots.Count, result.Count());
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByPartOfDayAsync_ReturnsFilteredTimeSlots()
    {
        // Arrange
        var partOfDay = "Morning";
        var expectedTimeSlots = new List<TimeSlot>
        {
            new TimeSlot { TimeSlotId = 1, StartTime = 800, EndTime = 900, PartOfDay = partOfDay }
        };
        _mockRepository.Setup(r => r.GetByPartOfDayAsync(partOfDay)).ReturnsAsync(expectedTimeSlots);

        // Act
        var result = await _service.GetByPartOfDayAsync(partOfDay);

        // Assert
        Assert.All(result, ts => Assert.Equal(partOfDay, ts.PartOfDay));
        _mockRepository.Verify(r => r.GetByPartOfDayAsync(partOfDay), Times.Once);
    }
}
