namespace PRM.Core.Entities;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<User> Users { get; set; } = [];
    public List<RolePermission> RolePermissions { get; set; } = [];
}
