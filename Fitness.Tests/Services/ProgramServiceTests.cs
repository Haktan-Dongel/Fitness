using Xunit;
using Moq;
using Fitness.Business.Models;
using Fitness.Business.Services;
using Fitness.Business.Interfaces;
using Fitness.Business.Exceptions;
using Microsoft.Extensions.Logging;

public class ProgramServiceTests
{
    private readonly Mock<IProgramRepository> _mockRepository;
    private readonly Mock<ILogger<ProgramService>> _mockLogger;
    private readonly ProgramService _service;

    public ProgramServiceTests()
    {
        _mockRepository = new Mock<IProgramRepository>();
        _mockLogger = new Mock<ILogger<ProgramService>>();
        _service = new ProgramService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task HasAvailableSpaceAsync_WhenSpaceAvailable_ReturnsTrue()
    {
        // Arrange
        var programCode = "FIT101";
        var program = new FitnessProgram { ProgramCode = programCode, MaxMembers = 10 };
        _mockRepository.Setup(r => r.GetCurrentMemberCountAsync(programCode)).ReturnsAsync(5);
        _mockRepository.Setup(r => r.GetByProgramCodeAsync(programCode)).ReturnsAsync(program);

        // Act
        var result = await _service.HasAvailableSpaceAsync(programCode);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasAvailableSpaceAsync_WhenFull_ReturnsFalse()
    {
        // Arrange
        var programCode = "FIT101";
        var program = new FitnessProgram { ProgramCode = programCode, MaxMembers = 10 };
        _mockRepository.Setup(r => r.GetCurrentMemberCountAsync(programCode)).ReturnsAsync(10);
        _mockRepository.Setup(r => r.GetByProgramCodeAsync(programCode)).ReturnsAsync(program);

        // Act
        var result = await _service.HasAvailableSpaceAsync(programCode);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasAvailableSpaceAsync_WhenProgramNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var programCode = "INVALID";
        _mockRepository.Setup(r => r.GetByProgramCodeAsync(programCode)).ReturnsAsync((FitnessProgram)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.HasAvailableSpaceAsync(programCode));
    }

    [Fact]
    public async Task AddMemberToProgramAsync_WhenValidAndSpaceAvailable_Succeeds()
    {
        // Arrange
        var programCode = "FIT101";
        var memberId = 1;
        var program = new FitnessProgram { ProgramCode = programCode, MaxMembers = 10 };

        _mockRepository.Setup(r => r.GetCurrentMemberCountAsync(programCode)).ReturnsAsync(5);
        _mockRepository.Setup(r => r.GetByProgramCodeAsync(programCode)).ReturnsAsync(program);
        _mockRepository.Setup(r => r.IsMemberEnrolledAsync(programCode, memberId)).ReturnsAsync(false);

        // Act
        await _service.AddMemberToProgramAsync(programCode, memberId);

        // Assert
        _mockRepository.Verify(r => r.AddMemberToProgramAsync(programCode, memberId), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task AddMemberToProgramAsync_WithInvalidProgramCode_ThrowsValidationException(string programCode)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.AddMemberToProgramAsync(programCode, 1));
    }

    [Fact]
    public async Task AddMemberToProgramAsync_WithInvalidMemberId_ThrowsValidationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.AddMemberToProgramAsync("FIT101", 0));
    }

    [Fact]
    public async Task AddMemberToProgramAsync_WhenMemberAlreadyEnrolled_ThrowsValidationException()
    {
        // Arrange
        var programCode = "FIT101";
        var memberId = 1;
        var program = new FitnessProgram { ProgramCode = programCode, MaxMembers = 10 };

        _mockRepository.Setup(r => r.GetCurrentMemberCountAsync(programCode)).ReturnsAsync(5);
        _mockRepository.Setup(r => r.GetByProgramCodeAsync(programCode)).ReturnsAsync(program);
        _mockRepository.Setup(r => r.IsMemberEnrolledAsync(programCode, memberId)).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.AddMemberToProgramAsync(programCode, memberId));
    }
}
