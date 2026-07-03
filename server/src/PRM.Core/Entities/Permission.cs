namespace PRM.Core.Entities;

public class Permission
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<RolePermission> RolePermissions { get; set; } = [];
}
