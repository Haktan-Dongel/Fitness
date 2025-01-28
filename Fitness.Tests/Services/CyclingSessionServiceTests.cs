using Xunit;
using Moq;
using Fitness.Business.Models;
using Fitness.Business.Services;
using Fitness.Business.Interfaces;
using Microsoft.Extensions.Logging;

public class CyclingSessionServiceTests
{
    private readonly Mock<ICyclingSessionRepository> _mockRepository;
    private readonly Mock<ILogger<CyclingSessionService>> _mockLogger;
    private readonly CyclingSessionService _service;

    public CyclingSessionServiceTests()
    {
        _mockRepository = new Mock<ICyclingSessionRepository>();
        _mockLogger = new Mock<ILogger<CyclingSessionService>>();
        _service = new CyclingSessionService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByMonthAsync_ReturnsSessionsForSpecificMonth()
    {
        // Arrange
        var memberId = 1;
        var year = 2024;
        var month = 1;
        var expectedSessions = new List<CyclingSession>
        {
            new CyclingSession { 
                CyclingSessionId = 1, 
                MemberId = memberId,
                Date = new DateTime(2024, 1, 15),
                Duration = 30,
                AvgWatt = 150,
                MaxWatt = 200
            }
        };

        _mockRepository.Setup(r => r.GetByDateRangeAsync(
            memberId,
            It.Is<DateTime>(d => d.Year == year && d.Month == month),
            It.IsAny<DateTime>()))
        .ReturnsAsync(expectedSessions);

        // Act
        var result = await _service.GetByMonthAsync(memberId, year, month);

        // Assert
        Assert.Single(result);
        Assert.Equal(expectedSessions[0].CyclingSessionId, result.First().CyclingSessionId);
    }

    [Fact]
    public async Task GetPerformanceStatsAsync_CalculatesCorrectStats()
    {
        // Arrange
        var memberId = 1;
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);
        var sessions = new List<CyclingSession>
        {
            new CyclingSession { AvgWatt = 100, MaxWatt = 150 },
            new CyclingSession { AvgWatt = 200, MaxWatt = 250 }
        };

        _mockRepository.Setup(r => r.GetByDateRangeAsync(memberId, startDate, endDate))
            .ReturnsAsync(sessions);

        // Act
        var result = await _service.GetPerformanceStatsAsync(memberId, startDate, endDate);

        // Assert
        Assert.Equal(150, result.AverageWatt);
        Assert.Equal(250, result.MaxWatt);
    }
}
