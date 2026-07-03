using Microsoft.EntityFrameworkCore;
using PRM.Core.Constants;
using PRM.Core.Entities;
using PRM.Core.Interfaces;

namespace PRM.Infrastructure.Data.Seed;

public static class AdminSeed
{
    public static async Task SeedAsync(PrmDbContext context, IPasswordHasher passwordHasher)
    {
        // 1. Seed Roles
        if (!await context.Roles.AnyAsync())
        {
            var roles = new List<Role>
            {
                new() { Name = "Admin", Description = "System Operator role" },
                new() { Name = "Manager", Description = "Delivery Manager role" },
                new() { Name = "Employee", Description = "Individual Contributor role" }
            };
            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        // 2. Seed Permissions
        if (!await context.Permissions.AnyAsync())
        {
            var permissions = new List<Permission>
            {
                new() { Name = "ManageUsers", Description = "Create/update/deactivate users" },
                new() { Name = "ManageEmployees", Description = "View/update/deactivate employees and manage skills" },
                new() { Name = "ManageProjects", Description = "Create/update projects and manage milestones" },
                new() { Name = "ViewAllAllocations", Description = "View company-wide resource allocations" },
                new() { Name = "SystemConfig", Description = "Update system configurations" },
                new() { Name = "ResourceDashboard", Description = "View manager's resource dashboard" },
                new() { Name = "AllocateResource", Description = "Allocate/deallocate resources on manager's projects" },
                new() { Name = "MyProjects", Description = "View manager's own projects and health metrics" },
                new() { Name = "ViewTeamTimesheets", Description = "Read team submitted timesheets" },
                new() { Name = "AiAssistant", Description = "Use AI skill matching and risk summary features" },
                new() { Name = "SubmitTimesheet", Description = "Submit weekly timesheets with entries" },
                new() { Name = "ViewMyTimesheets", Description = "View employee's own timesheet submission history" },
                new() { Name = "ViewMyAllocations", Description = "View employee's own allocations" }
            };
            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();

            // Link permissions to roles
            var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
            var managerRole = await context.Roles.FirstAsync(r => r.Name == "Manager");
            var employeeRole = await context.Roles.FirstAsync(r => r.Name == "Employee");

            var allPermissions = await context.Permissions.ToListAsync();

            // Admin permissions
            foreach (var perm in allPermissions.Where(p => p.Name is "ManageUsers" or "ManageEmployees" or "ManageProjects" or "ViewAllAllocations" or "SystemConfig"))
            {
                await context.RolePermissions.AddAsync(new RolePermission { RoleId = adminRole.Id, PermissionId = perm.Id });
            }

            // Manager permissions
            foreach (var perm in allPermissions.Where(p => p.Name is "ResourceDashboard" or "AllocateResource" or "MyProjects" or "ViewTeamTimesheets" or "AiAssistant"))
            {
                await context.RolePermissions.AddAsync(new RolePermission { RoleId = managerRole.Id, PermissionId = perm.Id });
            }

            // Employee permissions
            foreach (var perm in allPermissions.Where(p => p.Name is "SubmitTimesheet" or "ViewMyTimesheets" or "ViewMyAllocations"))
            {
                await context.RolePermissions.AddAsync(new RolePermission { RoleId = employeeRole.Id, PermissionId = perm.Id });
            }

            await context.SaveChangesAsync();
        }

        // 3. Seed Users
        if (!await context.Users.AnyAsync())
        {
            var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");

            var adminUser = new User
            {
                Username = AppConstants.DefaultAdminUsername,
                Email = AppConstants.DefaultAdminEmail,
                FullName = AppConstants.DefaultAdminFullName,
                PasswordHash = passwordHasher.Hash(AppConstants.DefaultAdminPassword),
                RoleId = adminRole.Id,
                IsActive = true,
                ForcePasswordChange = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();
        }

        // 4. Seed Configs
        if (!await context.SystemConfigs.AnyAsync())
        {
            var defaults = new List<SystemConfig>
            {
                new() { Key = AppConstants.ConfigKeyMaxWeeklyHours, Value = AppConstants.DefaultMaxWeeklyHours.ToString() },
                new() { Key = AppConstants.ConfigKeyLlmProvider, Value = "Gemini" },
                new() { Key = AppConstants.ConfigKeySchedulerIntervalMinutes, Value = "60" }
            };

            await context.SystemConfigs.AddRangeAsync(defaults);
            await context.SaveChangesAsync();
        }
    }
}
