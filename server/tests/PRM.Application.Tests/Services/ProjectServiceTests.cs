using FluentAssertions;
using Moq;
using PRM.Application.DTOs.Projects;
using PRM.Application.Services;
using PRM.Core.Entities;
using PRM.Core.Enums;
using PRM.Core.Exceptions;
using PRM.Core.Interfaces;
using Xunit;

namespace PRM.Application.Tests.Services;

public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly ProjectService _projectService;

    public ProjectServiceTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _projectService = new ProjectService(
            _projectRepositoryMock.Object,
            _userRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateProjectAsync_WithValidData_ReturnsProjectResponse()
    {
        // Arrange
        var request = new CreateProjectRequest(
            Name: "Test Project",
            Description: "Test Description",
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow.AddDays(30),
            Status: "Planned",
            ManagerId: null,
            TotalStoryPoints: 100
        );

        _projectRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Project>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _projectService.CreateProjectAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Project");
        result.Status.Should().Be("Planned");
        _projectRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Project>(p => p.Name == "Test Project")), Times.Once);
    }

    [Fact]
    public async Task CreateProjectAsync_WithInvalidDateRange_ThrowsDomainException()
    {
        // Arrange
        var request = new CreateProjectRequest(
            Name: "Test Project",
            Description: "Test Description",
            StartDate: DateTime.UtcNow.AddDays(30),
            EndDate: DateTime.UtcNow, // End date before start date
            Status: "Planned",
            ManagerId: null,
            TotalStoryPoints: 100
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _projectService.CreateProjectAsync(request));
        exception.ErrorCode.Should().Be("INVALID_DATE_RANGE");
    }

    [Fact]
    public async Task CreateProjectAsync_WithInvalidStatus_ThrowsDomainException()
    {
        // Arrange
        var request = new CreateProjectRequest(
            Name: "Test Project",
            Description: "Test Description",
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow.AddDays(30),
            Status: "InvalidStatus",
            ManagerId: null,
            TotalStoryPoints: 100
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _projectService.CreateProjectAsync(request));
        exception.ErrorCode.Should().Be("INVALID_STATUS");
    }

    [Fact]
    public async Task CreateProjectAsync_WithInvalidManager_ThrowsDomainException()
    {
        // Arrange
        var request = new CreateProjectRequest(
            Name: "Test Project",
            Description: "Test Description",
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow.AddDays(30),
            Status: "Planned",
            ManagerId: 1,
            TotalStoryPoints: 100
        );

        var user = new User { Id = 1, Role = new Role { Name = "Employee" } }; // Not a manager
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _projectService.CreateProjectAsync(request));
        exception.ErrorCode.Should().Be("INVALID_MANAGER");
    }

    [Fact]
    public async Task GetProjectByIdAsync_WithValidId_ReturnsProjectResponse()
    {
        // Arrange
        var project = new Project
        {
            Id = 1,
            Name = "Existing Project",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(10),
            Status = ProjectStatus.Active,
            Milestones = new List<Milestone>()
        };

        _projectRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(project);

        // Act
        var result = await _projectService.GetProjectByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Existing Project");
    }

    [Fact]
    public async Task GetProjectByIdAsync_WithInvalidId_ThrowsDomainException()
    {
        // Arrange
        _projectRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((Project)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _projectService.GetProjectByIdAsync(1));
        exception.ErrorCode.Should().Be("PROJECT_NOT_FOUND");
    }
    [Fact]
    public async Task GetAllProjectsAsync_ReturnsProjectList()
    {
        var projects = new List<Project>
        {
            new Project { Id = 1, Name = "Project 1", Milestones = new List<Milestone>() },
            new Project { Id = 2, Name = "Project 2", Milestones = new List<Milestone>() }
        };
        _projectRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(projects);

        var result = await _projectService.GetAllProjectsAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateProjectAsync_WithValidData_UpdatesProject()
    {
        var request = new UpdateProjectRequest(
            Name: "Updated Project",
            Description: "Updated Description",
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow.AddDays(30),
            Status: "Active",
            ManagerId: null,
            TotalStoryPoints: 120
        );

        var project = new Project
        {
            Id = 1,
            Name = "Old Project",
            Status = ProjectStatus.Planned
        };

        _projectRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(project);
        _projectRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Project>())).Returns(Task.CompletedTask);

        await _projectService.UpdateProjectAsync(1, request);

        project.Name.Should().Be("Updated Project");
        project.Status.Should().Be(ProjectStatus.Active);
        _projectRepositoryMock.Verify(repo => repo.UpdateAsync(project), Times.Once);
    }
    [Fact]
    public async Task GetProjectsByManagerIdAsync_ReturnsProjectList()
    {
        var projects = new List<Project>
        {
            new Project { Id = 1, ManagerId = 1, Milestones = new List<Milestone>() }
        };

        _projectRepositoryMock.Setup(repo => repo.GetByManagerIdAsync(1))
            .ReturnsAsync(projects);

        var result = await _projectService.GetProjectsByManagerIdAsync(1);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddMilestoneAsync_WithValidData_AddsMilestone()
    {
        var request = new AddMilestoneRequest(
            Title: "M1",
            DueDate: DateTime.UtcNow.AddDays(10),
            StoryPoints: 50
        );

        _projectRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(new Project { Id = 1 });
        _projectRepositoryMock.Setup(repo => repo.AddMilestoneAsync(It.IsAny<Milestone>())).Returns(Task.CompletedTask);

        await _projectService.AddMilestoneAsync(1, request);

        _projectRepositoryMock.Verify(repo => repo.AddMilestoneAsync(It.Is<Milestone>(m => m.Title == "M1")), Times.Once);
    }

    [Fact]
    public async Task UpdateMilestoneStatusAsync_WithValidData_UpdatesStatus()
    {
        var milestone = new Milestone { Id = 1, Status = MilestoneStatus.NotStarted };
        var request = new UpdateMilestoneStatusRequest("InProgress");

        _projectRepositoryMock.Setup(repo => repo.GetMilestoneByIdAsync(1)).ReturnsAsync(milestone);
        _projectRepositoryMock.Setup(repo => repo.UpdateMilestoneAsync(milestone)).Returns(Task.CompletedTask);

        await _projectService.UpdateMilestoneStatusAsync(1, request);

        milestone.Status.Should().Be(MilestoneStatus.InProgress);
        _projectRepositoryMock.Verify(repo => repo.UpdateMilestoneAsync(milestone), Times.Once);
    }

    [Fact]
    public async Task GetMilestonesByProjectIdAsync_ReturnsMilestoneList()
    {
        var milestones = new List<Milestone> { new Milestone { Id = 1, ProjectId = 1, Title = "M1" } };
        var project = new Project { Id = 1, Milestones = milestones };

        _projectRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(project);

        var result = await _projectService.GetMilestonesByProjectIdAsync(1);

        result.Should().HaveCount(1);
        result.First().Title.Should().Be("M1");
    }

    [Fact]
    public async Task UpdateProjectAsync_WithInvalidId_ThrowsDomainException()
    {
        var request = new UpdateProjectRequest("Updated", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddDays(10), "Active", null, 100);
        _projectRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((Project)null);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _projectService.UpdateProjectAsync(1, request));
        exception.ErrorCode.Should().Be("PROJECT_NOT_FOUND");
    }

    [Fact]
    public async Task AddMilestoneAsync_WithInvalidProject_ThrowsDomainException()
    {
        var request = new AddMilestoneRequest("M1", DateTime.UtcNow.AddDays(10), 50);
        _projectRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((Project)null);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _projectService.AddMilestoneAsync(1, request));
        exception.ErrorCode.Should().Be("PROJECT_NOT_FOUND");
    }

    [Fact]
    public async Task UpdateMilestoneStatusAsync_WithInvalidMilestone_ThrowsDomainException()
    {
        var request = new UpdateMilestoneStatusRequest("InProgress");
        _projectRepositoryMock.Setup(repo => repo.GetMilestoneByIdAsync(1)).ReturnsAsync((Milestone)null);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _projectService.UpdateMilestoneStatusAsync(1, request));
        exception.ErrorCode.Should().Be("MILESTONE_NOT_FOUND");
    }

    [Fact]
    public async Task GetMilestonesByProjectIdAsync_WithInvalidProject_ThrowsDomainException()
    {
        _projectRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((Project)null);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _projectService.GetMilestonesByProjectIdAsync(1));
        exception.ErrorCode.Should().Be("PROJECT_NOT_FOUND");
    }
}
