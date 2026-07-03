namespace PRM.Core.Enums;

/// <summary>
/// Indicates whether a user/employee currently has active resource allocations.
/// Computed when allocations change (add/end).
/// </summary>
public enum EmployeeStatus
{
    Bench,
    Allocated
}
