using FluentAssertions;
using Moq;
using PRM.Application.DTOs.Timesheets;
using PRM.Application.Services;
using PRM.Core.Constants;
using PRM.Core.Entities;
using PRM.Core.Enums;
using PRM.Core.Exceptions;
using PRM.Core.Interfaces;
using Xunit;

namespace PRM.Application.Tests.Services;

public class TimesheetServiceTests
{
    private readonly Mock<ITimesheetRepository> _timesheetRepositoryMock;
    private readonly Mock<IAllocationRepository> _allocationRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ISystemConfigRepository> _configRepositoryMock;
    private readonly TimesheetService _timesheetService;

    public TimesheetServiceTests()
    {
        _timesheetRepositoryMock = new Mock<ITimesheetRepository>();
        _allocationRepositoryMock = new Mock<IAllocationRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _configRepositoryMock = new Mock<ISystemConfigRepository>();

        _timesheetService = new TimesheetService(
            _timesheetRepositoryMock.Object,
            _allocationRepositoryMock.Object,
            _userRepositoryMock.Object,
            _configRepositoryMock.Object);
    }

    [Fact]
    public async Task SubmitTimesheetAsync_WithFutureWeek_ThrowsDomainException()
    {
        // Arrange
        var request = new SubmitTimesheetRequest(
            UserId: 1,
            WeekStartDate: DateTime.UtcNow.AddDays(10), // Future date
            Entries: new List<TimesheetEntryDto>()
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _timesheetService.SubmitTimesheetAsync(request));
        exception.ErrorCode.Should().Be("FUTURE_TIMESHEET");
    }

    [Fact]
    public async Task SubmitTimesheetAsync_WithInactiveUser_ThrowsDomainException()
    {
        // Arrange
        var request = new SubmitTimesheetRequest(
            UserId: 1,
            WeekStartDate: DateTime.UtcNow.Date.AddDays(-1),
            Entries: new List<TimesheetEntryDto>()
        );

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(new User { Id = 1, IsActive = false });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _timesheetService.SubmitTimesheetAsync(request));
        exception.ErrorCode.Should().Be("EMPLOYEE_NOT_FOUND");
    }

    [Fact]
    public async Task SubmitTimesheetAsync_ExceedsMaxWeeklyHours_ThrowsDomainException()
    {
        // Arrange
        var request = new SubmitTimesheetRequest(
            UserId: 1,
            WeekStartDate: DateTime.UtcNow.Date.AddDays(-1),
            Entries: new List<TimesheetEntryDto>
            {
                new TimesheetEntryDto(ProjectId: 1, HoursWorked: 50, ActivityTags: "Dev")
            }
        );

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(new User { Id = 1, IsActive = true });

        _configRepositoryMock.Setup(repo => repo.GetByKeyAsync(AppConstants.ConfigKeyMaxWeeklyHours))
            .ReturnsAsync(new SystemConfig { Value = "40" });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _timesheetService.SubmitTimesheetAsync(request));
        exception.ErrorCode.Should().Be("EXCEEDS_MAX_WEEKLY_HOURS");
    }

    [Fact]
    public async Task SubmitTimesheetAsync_WithValidData_ReturnsTimesheetResponse()
    {
        // Arrange
        var weekStart = DateTime.UtcNow.Date.AddDays(-1);
        var request = new SubmitTimesheetRequest(
            UserId: 1,
            WeekStartDate: weekStart,
            Entries: new List<TimesheetEntryDto>
            {
                new TimesheetEntryDto(ProjectId: 1, HoursWorked: 20, ActivityTags: "Dev")
            }
        );

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(new User { Id = 1, IsActive = true, FullName = "Test User" });

        _configRepositoryMock.Setup(repo => repo.GetByKeyAsync(AppConstants.ConfigKeyMaxWeeklyHours))
            .ReturnsAsync(new SystemConfig { Value = "40" });

        _timesheetRepositoryMock.Setup(repo => repo.GetByEmployeeAndWeekAsync(1, weekStart))
            .ReturnsAsync((Timesheet)null);

        var allocations = new List<Allocation>
        {
            new Allocation { ProjectId = 1, UtilisationPercent = 100 }
        };

        _allocationRepositoryMock.Setup(repo => repo.GetOverlappingAllocationsAsync(1, weekStart, It.IsAny<DateTime>()))
            .ReturnsAsync(allocations);

        _timesheetRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Timesheet>()))
            .Returns(Task.CompletedTask);

        _timesheetRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Timesheet
            {
                Id = 1,
                UserId = 1,
                User = new User { FullName = "Test User" },
                WeekStartDate = weekStart,
                Status = TimesheetStatus.Submitted,
                Entries = new List<TimesheetEntry>
                {
                    new TimesheetEntry { ProjectId = 1, HoursWorked = 20, Project = new Project { Name = "Test Project" } }
                }
            });

        // Act
        var result = await _timesheetService.SubmitTimesheetAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(TimesheetStatus.Submitted.ToString());
        result.Entries.Should().HaveCount(1);
    }
    [Fact]
    public async Task GetTimesheetsByEmployeeIdAsync_ReturnsTimesheetList()
    {
        var timesheets = new List<Timesheet>
        {
            new Timesheet { Id = 1, UserId = 1, Entries = new List<TimesheetEntry>() },
            new Timesheet { Id = 2, UserId = 1, Entries = new List<TimesheetEntry>() }
        };

        _timesheetRepositoryMock.Setup(repo => repo.GetByEmployeeIdAsync(1))
            .ReturnsAsync(timesheets);

        var result = await _timesheetService.GetTimesheetsByEmployeeIdAsync(1);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTeamTimesheetsAsync_ReturnsTimesheetList()
    {
        var timesheets = new List<Timesheet>
        {
            new Timesheet { Id = 1, UserId = 2, Entries = new List<TimesheetEntry>() }
        };

        var date = DateTime.UtcNow.Date;
        _timesheetRepositoryMock.Setup(repo => repo.GetTeamTimesheetsAsync(1, date))
            .ReturnsAsync(timesheets);

        var result = await _timesheetService.GetTeamTimesheetsAsync(1, date);

        result.Should().HaveCount(1);
    }
    [Fact]
    public async Task UpdateTimesheetStatusAsync_WithValidStatus_UpdatesStatus()
    {
        var timesheet = new Timesheet { Id = 1, Status = TimesheetStatus.Submitted };
        _timesheetRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(timesheet);

        await _timesheetService.UpdateTimesheetStatusAsync(1, "Missed");

        timesheet.Status.Should().Be(TimesheetStatus.Missed);
        _timesheetRepositoryMock.Verify(repo => repo.UpdateAsync(timesheet), Times.Once);
    }

    [Fact]
    public async Task UpdateTimesheetStatusAsync_WithInvalidId_ThrowsDomainException()
    {
        _timesheetRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((Timesheet)null);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _timesheetService.UpdateTimesheetStatusAsync(1, "Missed"));
        exception.ErrorCode.Should().Be("TIMESHEET_NOT_FOUND");
    }

    [Fact]
    public async Task SubmitTimesheetAsync_WithInvalidUser_ThrowsDomainException()
    {
        var request = new SubmitTimesheetRequest(1, DateTime.UtcNow, new List<TimesheetEntryDto>());
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((User)null);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _timesheetService.SubmitTimesheetAsync(request));
        exception.ErrorCode.Should().Be("EMPLOYEE_NOT_FOUND");
    }
}
