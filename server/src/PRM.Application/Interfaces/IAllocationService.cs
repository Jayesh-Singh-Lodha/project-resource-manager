using PRM.Application.DTOs.Allocations;

namespace PRM.Application.Interfaces;

public interface IAllocationService
{
    Task<AllocationResponse> AllocateResourceAsync(CreateAllocationRequest request);
    Task EndAllocationAsync(int id);
    Task<IReadOnlyList<AllocationResponse>> GetAllAllocationsAsync();
    Task<IReadOnlyList<AllocationResponse>> GetAllocationsByEmployeeIdAsync(int employeeId);
    Task<IReadOnlyList<AllocationResponse>> GetAllocationsByProjectIdAsync(int projectId);
}
