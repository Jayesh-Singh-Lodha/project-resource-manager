using FluentAssertions;
using Moq;
using PRM.Application.DTOs.Allocations;
using PRM.Application.Services;
using PRM.Core.Entities;
using PRM.Core.Enums;
using PRM.Core.Exceptions;
using PRM.Core.Interfaces;
using Xunit;

namespace PRM.Application.Tests.Services;

public class AllocationServiceTests
{
    private readonly Mock<IAllocationRepository> _allocationRepositoryMock;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly AllocationService _allocationService;

    public AllocationServiceTests()
    {
        _allocationRepositoryMock = new Mock<IAllocationRepository>();
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _allocationService = new AllocationService(
            _allocationRepositoryMock.Object,
            _projectRepositoryMock.Object,
            _userRepositoryMock.Object);
    }

    [Fact]
    public async Task AllocateResourceAsync_WithInvalidDateRange_ThrowsDomainException()
    {
        // Arrange
        var request = new CreateAllocationRequest(
            UserId: 1,
            ProjectId: 1,
            UtilisationPercent: 50,
            FromDate: DateTime.UtcNow.AddDays(10),
            ToDate: DateTime.UtcNow // ToDate before FromDate
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _allocationService.AllocateResourceAsync(request));
        exception.ErrorCode.Should().Be("INVALID_DATE_RANGE");
    }

    [Fact]
    public async Task AllocateResourceAsync_WithInvalidProjectStatus_ThrowsDomainException()
    {
        // Arrange
        var request = new CreateAllocationRequest(
            UserId: 1,
            ProjectId: 1,
            UtilisationPercent: 50,
            FromDate: DateTime.UtcNow,
            ToDate: DateTime.UtcNow.AddDays(30)
        );

        _projectRepositoryMock.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(new Project { Id = 1, Status = ProjectStatus.Completed }); // Not Active/Planned

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _allocationService.AllocateResourceAsync(request));
        exception.ErrorCode.Should().Be("PROJECT_NOT_ALLOCATABLE");
    }

    [Fact]
    public async Task AllocateResourceAsync_ExceedsCapacity_ThrowsDomainException()
    {
        // Arrange
        var request = new CreateAllocationRequest(
            UserId: 1,
            ProjectId: 1,
            UtilisationPercent: 60,
            FromDate: DateTime.UtcNow,
            ToDate: DateTime.UtcNow.AddDays(30)
        );

        _projectRepositoryMock.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(new Project { Id = 1, Status = ProjectStatus.Active });

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(new User { Id = 1, IsActive = true });

        var existingAllocations = new List<Allocation>
        {
            new Allocation
            {
                FromDate = DateTime.UtcNow.AddDays(-10),
                ToDate = DateTime.UtcNow.AddDays(10),
                UtilisationPercent = 50 // 50 + 60 = 110 (Exceeds 100)
            }
        };

        _allocationRepositoryMock.Setup(repo => repo.GetOverlappingAllocationsAsync(1, request.FromDate, request.ToDate))
            .ReturnsAsync(existingAllocations);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _allocationService.AllocateResourceAsync(request));
        exception.ErrorCode.Should().Be("CAPACITY_EXCEEDED");
    }

    [Fact]
    public async Task AllocateResourceAsync_WithValidData_ReturnsAllocationResponse()
    {
        // Arrange
        var request = new CreateAllocationRequest(
            UserId: 1,
            ProjectId: 1,
            UtilisationPercent: 50,
            FromDate: DateTime.UtcNow,
            ToDate: DateTime.UtcNow.AddDays(30)
        );

        var user = new User { Id = 1, IsActive = true, FullName = "Test User" };
        var project = new Project { Id = 1, Status = ProjectStatus.Active, Name = "Test Project" };

        _projectRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(project);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(user);
        _allocationRepositoryMock.Setup(repo => repo.GetOverlappingAllocationsAsync(1, request.FromDate, request.ToDate))
            .ReturnsAsync(new List<Allocation>());

        _allocationRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Allocation>()))
            .Returns(Task.CompletedTask);

        _allocationRepositoryMock.Setup(repo => repo.GetByEmployeeIdAsync(1))
            .ReturnsAsync(new List<Allocation> { new Allocation { ToDate = request.ToDate } });

        _allocationRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Allocation
            {
                Id = 1,
                UserId = 1,
                User = user,
                ProjectId = 1,
                Project = project,
                UtilisationPercent = 50,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            });

        // Act
        var result = await _allocationService.AllocateResourceAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.UtilisationPercent.Should().Be(50);
        result.UserName.Should().Be("Test User");
        user.Status.Should().Be(EmployeeStatus.Allocated);
    }
    [Fact]
    public async Task GetAllAllocationsAsync_ReturnsAllocationList()
    {
        var allocations = new List<Allocation>
        {
            new Allocation { Id = 1, User = new User(), Project = new Project() },
            new Allocation { Id = 2, User = new User(), Project = new Project() }
        };

        _allocationRepositoryMock.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(allocations);

        var result = await _allocationService.GetAllAllocationsAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllocationsByEmployeeIdAsync_ReturnsAllocationList()
    {
        var allocations = new List<Allocation>
        {
            new Allocation { Id = 1, UserId = 1, User = new User(), Project = new Project() }
        };

        _allocationRepositoryMock.Setup(repo => repo.GetByEmployeeIdAsync(1))
            .ReturnsAsync(allocations);

        var result = await _allocationService.GetAllocationsByEmployeeIdAsync(1);

        result.Should().HaveCount(1);
    }
    [Fact]
    public async Task EndAllocationAsync_WithValidId_EndsAllocation()
    {
        var allocation = new Allocation { Id = 1, UserId = 1, ToDate = DateTime.UtcNow.AddDays(10) };
        var user = new User { Id = 1, Status = EmployeeStatus.Allocated };

        _allocationRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(allocation);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(user);
        _allocationRepositoryMock.Setup(repo => repo.GetByEmployeeIdAsync(1))
            .ReturnsAsync(new List<Allocation> { new Allocation { ToDate = DateTime.UtcNow.AddDays(-1) } }); // No future allocations

        await _allocationService.EndAllocationAsync(1);

        allocation.ToDate.Date.Should().Be(DateTime.UtcNow.Date.AddDays(-1));
        user.Status.Should().Be(EmployeeStatus.Bench);
        _allocationRepositoryMock.Verify(repo => repo.UpdateAsync(allocation), Times.Once);
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task EndAllocationAsync_WithInvalidId_ThrowsDomainException()
    {
        _allocationRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((Allocation)null);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _allocationService.EndAllocationAsync(1));
        exception.ErrorCode.Should().Be("ALLOCATION_NOT_FOUND");
    }
}
