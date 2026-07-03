using PRM.Application.DTOs.Allocations;
using PRM.Application.Interfaces;
using PRM.Core.Entities;
using PRM.Core.Enums;
using PRM.Core.Exceptions;
using PRM.Core.Interfaces;

namespace PRM.Application.Services;

public class AllocationService : IAllocationService
{
    private readonly IAllocationRepository _allocationRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;

    public AllocationService(
        IAllocationRepository allocationRepository,
        IProjectRepository projectRepository,
        IUserRepository userRepository)
    {
        _allocationRepository = allocationRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
    }

    public async Task<AllocationResponse> AllocateResourceAsync(CreateAllocationRequest request)
    {
        if (request.FromDate > request.ToDate)
        {
            throw new DomainException("Start date must be before or equal to end date.", "INVALID_DATE_RANGE");
        }

        if (request.UtilisationPercent <= 0 || request.UtilisationPercent > 100)
        {
            throw new DomainException("Utilisation percentage must be between 1 and 100.", "INVALID_UTILISATION");
        }

        var project = await _projectRepository.GetByIdAsync(request.ProjectId);
        if (project is null)
        {
            throw new DomainException("Project not found.", "PROJECT_NOT_FOUND");
        }

        if (project.Status != ProjectStatus.Active && project.Status != ProjectStatus.Planned)
        {
            throw new DomainException("Allocations can only be made on Active or Planned projects.", "PROJECT_NOT_ALLOCATABLE");
        }

        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user is null || !user.IsActive)
        {
            throw new DomainException("Employee not found or is inactive.", "EMPLOYEE_NOT_ALLOCATABLE");
        }

        if (user.ManagerId != project.ManagerId)
        {
            throw new DomainException("Employee can only be allocated to projects managed by their reporting manager.", "CROSS_TEAM_ALLOCATION");
        }

        // Validate overlapping capacity
        var overlapping = await _allocationRepository.GetOverlappingAllocationsAsync(request.UserId, request.FromDate, request.ToDate);
        var datesToCheck = new List<DateTime> { request.FromDate, request.ToDate };
        foreach (var o in overlapping)
        {
            if (o.FromDate >= request.FromDate && o.FromDate <= request.ToDate) datesToCheck.Add(o.FromDate);
            if (o.ToDate >= request.FromDate && o.ToDate <= request.ToDate) datesToCheck.Add(o.ToDate);
        }

        foreach (var date in datesToCheck.Distinct())
        {
            var sum = overlapping.Where(o => o.FromDate <= date && o.ToDate >= date).Sum(o => o.UtilisationPercent);
            if (sum + request.UtilisationPercent > 100)
            {
                throw new DomainException($"Allocation would exceed 100% capacity on {date:dd-MM-yyyy}. Current allocation on this day is {sum}%.", "CAPACITY_EXCEEDED");
            }
        }

        var allocation = new Allocation
        {
            UserId = request.UserId,
            ProjectId = request.ProjectId,
            UtilisationPercent = request.UtilisationPercent,
            FromDate = request.FromDate,
            ToDate = request.ToDate
        };

        await _allocationRepository.AddAsync(allocation);

        // Update user status
        var allAllocations = await _allocationRepository.GetByEmployeeIdAsync(request.UserId);
        user.Status = allAllocations.Any(a => a.ToDate >= DateTime.UtcNow.Date)
            ? EmployeeStatus.Allocated
            : EmployeeStatus.Bench;

        await _userRepository.UpdateAsync(user);

        // Fetch populated object to map response
        var saved = await _allocationRepository.GetByIdAsync(allocation.Id);
        return MapToResponse(saved!);
    }

    public async Task EndAllocationAsync(int id)
    {
        var allocation = await _allocationRepository.GetByIdAsync(id);
        if (allocation is null)
        {
            throw new DomainException("Allocation not found.", "ALLOCATION_NOT_FOUND");
        }

        allocation.ToDate = DateTime.UtcNow.Date.AddDays(-1);
        await _allocationRepository.UpdateAsync(allocation);

        // Update user status
        var user = await _userRepository.GetByIdAsync(allocation.UserId);
        if (user is not null)
        {
            var allAllocations = await _allocationRepository.GetByEmployeeIdAsync(user.Id);
            user.Status = allAllocations.Any(a => a.ToDate >= DateTime.UtcNow.Date)
                ? EmployeeStatus.Allocated
                : EmployeeStatus.Bench;

            await _userRepository.UpdateAsync(user);
        }
    }

    public async Task<IReadOnlyList<AllocationResponse>> GetAllAllocationsAsync()
    {
        var allocations = await _allocationRepository.GetAllAsync();
        return allocations.Select(MapToResponse).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<AllocationResponse>> GetAllocationsByEmployeeIdAsync(int employeeId)
    {
        var allocations = await _allocationRepository.GetByEmployeeIdAsync(employeeId);
        return allocations.Select(MapToResponse).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<AllocationResponse>> GetAllocationsByProjectIdAsync(int projectId)
    {
        var allocations = await _allocationRepository.GetByProjectIdAsync(projectId);
        return allocations.Select(MapToResponse).ToList().AsReadOnly();
    }

    private static AllocationResponse MapToResponse(Allocation a)
    {
        return new AllocationResponse(
            Id: a.Id,
            UserId: a.UserId,
            UserName: a.User.FullName,
            ProjectId: a.ProjectId,
            ProjectName: a.Project.Name,
            UtilisationPercent: a.UtilisationPercent,
            FromDate: a.FromDate,
            ToDate: a.ToDate
        );
    }
}
