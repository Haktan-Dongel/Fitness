using Xunit;
using Moq;
using Fitness.Business.Models;
using Fitness.Business.Services;
using Fitness.Business.Interfaces;
using Microsoft.Extensions.Logging;

public class RunningSessionServiceTests
{
    private readonly Mock<IRunningSessionRepository> _mockRepository;
    private readonly Mock<ILogger<RunningSessionService>> _mockLogger;
    private readonly RunningSessionService _service;

    public RunningSessionServiceTests()
    {
        _mockRepository = new Mock<IRunningSessionRepository>();
        _mockLogger = new Mock<ILogger<RunningSessionService>>();
        _service = new RunningSessionService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByMonthAsync_ReturnsSessionsForSpecificMonth()
    {
        // Arrange
        var memberId = 1;
        var year = 2024;
        var month = 1;
        var expectedSessions = new List<RunningSessionMain>
        {
            new RunningSessionMain 
            { 
                RunningSessionId = 1,
                MemberId = memberId,
                Date = new DateTime(2024, 1, 15),
                Duration = 30,
                AvgSpeed = 10
            }
        };

        _mockRepository.Setup(r => r.GetByMonthAsync(memberId, year, month))
            .ReturnsAsync(expectedSessions);

        // Act
        var result = await _service.GetByMonthAsync(memberId, year, month);

        // Assert
        Assert.Single(result);
        Assert.Equal(expectedSessions[0].RunningSessionId, result.First().RunningSessionId);
    }

    [Fact]
    public async Task GetPerformanceStatsAsync_ReturnsCorrectStats()
    {
        // Arrange
        var memberId = 1;
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);
        var expectedStats = (TotalDistance: 5.0, AverageSpeed: 10.0);

        _mockRepository.Setup(r => r.GetPerformanceStatsAsync(memberId, startDate, endDate))
            .ReturnsAsync(expectedStats);

        // Act
        var result = await _service.GetPerformanceStatsAsync(memberId, startDate, endDate);

        // Assert
        Assert.Equal(expectedStats.TotalDistance, result.TotalDistance);
        Assert.Equal(expectedStats.AverageSpeed, result.AverageSpeed);
    }

    [Fact]
    public async Task GetByDateRangeAsync_ReturnsCorrectSessions()
    {
        // Arrange
        var memberId = 1;
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);
        var expectedSessions = new List<RunningSessionMain>
        {
            new RunningSessionMain 
            { 
                RunningSessionId = 1,
                Date = new DateTime(2024, 1, 15),
                Duration = 30,
                AvgSpeed = 10
            }
        };

        _mockRepository.Setup(r => r.GetByDateRangeAsync(memberId, startDate, endDate))
            .ReturnsAsync(expectedSessions);

        // Act
        var result = await _service.GetByDateRangeAsync(memberId, startDate, endDate);

        // Assert
        Assert.Single(result);
        Assert.Equal(expectedSessions[0].RunningSessionId, result.First().RunningSessionId);
    }
}
