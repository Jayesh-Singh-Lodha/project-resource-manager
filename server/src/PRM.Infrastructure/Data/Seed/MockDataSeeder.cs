using Bogus;
using Microsoft.EntityFrameworkCore;
using PRM.Core.Constants;
using PRM.Core.Entities;
using PRM.Core.Enums;
using PRM.Core.Interfaces;

namespace PRM.Infrastructure.Data.Seed;

public static class MockDataSeeder
{
    public static async Task SeedMockDataAsync(PrmDbContext context, IPasswordHasher passwordHasher)
    {
        if (await context.Users.CountAsync() > 10)
        {
            return;
        }

        var roles = await context.Roles.ToListAsync();
        var managerRole = roles.FirstOrDefault(r => r.Name == "Manager");
        var employeeRole = roles.FirstOrDefault(r => r.Name == "Employee");

        if (managerRole == null || employeeRole == null)
            return;

        var defaultPasswordHash = passwordHasher.Hash("Password123!");

        // Tech-specific data lists for better mock data
        var techDepartments = new[] { "Engineering", "Architecture", "Data Science", "Quality Assurance", "Cloud & DevOps", "Product Development" };
        var techProjects = new[] { 
            "Cloud Migration to AWS", "Microservices Architecture Refactoring", "AI Customer Service Bot", 
            "Legacy ERP Replacement", "Mobile App v2.0 React Native", "Data Lake Infrastructure", 
            "Real-time Analytics Dashboard", "E-commerce Backend Rewrite (Go)", "CI/CD Pipeline Automation", 
            "Zero-Trust Security Implementation", "IoT Device Integration API", "Blockchain Ledger Service",
            "Internal HR Portal", "Customer CRM Migration", "Automated Load Testing Suite",
            "Payment Gateway Integration", "Serverless Functions API", "WebAssembly Web Client",
            "GraphQL Federation Layer", "Distributed Caching System" 
        };
        var techJobTitles = new[] { 
            "Backend Engineer", "Frontend Engineer", "Full Stack Developer", "Data Scientist", 
            "DevOps Engineer", "QA Automation Engineer", "Scrum Master", "Cloud Architect", 
            "Security Analyst", "Systems Engineer" 
        };
        var techTaskDescriptions = new[] {
            "Wrote unit tests for API endpoints", "Debugged memory leak in production", 
            "Set up CI/CD pipeline in GitHub Actions", "Refactored legacy controllers",
            "Migrated database schema for new features", "Optimized SQL queries for performance",
            "Developed React components for UI", "Integrated third-party payment gateway",
            "Configured Kubernetes cluster", "Conducted code review for team members",
            "Resolved merge conflicts and deployed to staging", "Wrote documentation for new architecture"
        };

        Randomizer.Seed = new Random(42);

        // 1. Seed Skills
        var skillsData = new[]
        {
            ("C# / .NET", SkillCategory.Backend),
            ("Java / Spring Boot", SkillCategory.Backend),
            ("Node.js / Express", SkillCategory.Backend),
            ("Python", SkillCategory.Backend),
            ("React", SkillCategory.Frontend),
            ("Angular", SkillCategory.Frontend),
            ("Vue.js", SkillCategory.Frontend),
            ("SQL Server", SkillCategory.Other),
            ("PostgreSQL", SkillCategory.Other),
            ("MongoDB", SkillCategory.Other),
            ("Azure", SkillCategory.DevOps),
            ("AWS", SkillCategory.DevOps),
            ("Docker & Kubernetes", SkillCategory.DevOps),
            ("Agile & Scrum", SkillCategory.Other),
            ("Selenium / Cypress", SkillCategory.QA)
        };

        var skills = new List<Skill>();
        foreach (var s in skillsData)
        {
            skills.Add(new Skill
            {
                Name = s.Item1,
                Category = s.Item2
            });
        }
        await context.Skills.AddRangeAsync(skills);
        await context.SaveChangesAsync();

        // 2. Seed Managers
        var managerFaker = new Faker<User>()
            .RuleFor(u => u.Username, f => $"manager{f.IndexFaker}")
            .RuleFor(u => u.Email, f => $"manager{f.IndexFaker}@prm.local")
            .RuleFor(u => u.FullName, f => f.Name.FullName())
            .RuleFor(u => u.PasswordHash, defaultPasswordHash)
            .RuleFor(u => u.RoleId, managerRole.Id)
            .RuleFor(u => u.Department, f => f.PickRandom(techDepartments))
            .RuleFor(u => u.Status, EmployeeStatus.Allocated)
            .RuleFor(u => u.IsActive, true)
            .RuleFor(u => u.ForcePasswordChange, false)
            .RuleFor(u => u.CreatedAt, f => f.Date.Past(1).ToUniversalTime())
            .RuleFor(u => u.UpdatedAt, f => f.Date.Recent().ToUniversalTime());

        var managers = managerFaker.Generate(5);
        await context.Users.AddRangeAsync(managers);
        await context.SaveChangesAsync();

        // 3. Seed Employees
        var employeeFaker = new Faker<User>()
            .RuleFor(u => u.Username, f => $"employee{f.IndexFaker}")
            .RuleFor(u => u.Email, f => $"employee{f.IndexFaker}@prm.local")
            .RuleFor(u => u.FullName, f => f.Name.FullName())
            .RuleFor(u => u.PasswordHash, defaultPasswordHash)
            .RuleFor(u => u.RoleId, employeeRole.Id)
            .RuleFor(u => u.Department, f => f.PickRandom(techDepartments))
            .RuleFor(u => u.ManagerId, f => f.PickRandom(managers).Id)
            .RuleFor(u => u.Status, EmployeeStatus.Bench)
            .RuleFor(u => u.IsActive, true)
            .RuleFor(u => u.ForcePasswordChange, false)
            .RuleFor(u => u.CreatedAt, f => f.Date.Past(1).ToUniversalTime())
            .RuleFor(u => u.UpdatedAt, f => f.Date.Recent().ToUniversalTime());

        var employees = employeeFaker.Generate(50);
        await context.Users.AddRangeAsync(employees);
        await context.SaveChangesAsync();

        // 4. Seed UserSkills
        var faker = new Faker();
        var userSkills = new List<UserSkill>();
        foreach (var emp in employees)
        {
            var empSkills = faker.PickRandom(skills, faker.Random.Int(2, 5)).ToList();
            foreach (var s in empSkills)
            {
                userSkills.Add(new UserSkill
                {
                    UserId = emp.Id,
                    SkillId = s.Id,
                    ProficiencyLevel = faker.PickRandom<ProficiencyLevel>()
                });
            }
        }
        await context.UserSkills.AddRangeAsync(userSkills);
        await context.SaveChangesAsync();

        // 5. Seed Projects
        var projectFaker = new Faker<Project>()
            .RuleFor(p => p.Name, f => f.PickRandom(techProjects))
            .RuleFor(p => p.Description, f => f.Lorem.Paragraph())
            .RuleFor(p => p.StartDate, f => f.Date.Recent(60).ToUniversalTime())
            .RuleFor(p => p.EndDate, (f, p) => p.StartDate.AddDays(f.Random.Int(60, 240)))
            .RuleFor(p => p.Status, f => f.PickRandom<ProjectStatus>())
            .RuleFor(p => p.ManagerId, f => f.PickRandom(managers).Id)
            .RuleFor(p => p.HealthStatus, f => f.PickRandom<HealthStatus>())
            .RuleFor(p => p.CreatedAt, f => f.Date.Past().ToUniversalTime())
            .RuleFor(p => p.UpdatedAt, f => f.Date.Recent().ToUniversalTime());

        var projects = projectFaker.Generate(20);
        // Ensure unique names if possible, but PickRandom on 20 from 20 might duplicate. That's fine for mock data.
        await context.Projects.AddRangeAsync(projects);
        await context.SaveChangesAsync();

        // 6. Seed Milestones
        var milestones = new List<Milestone>();
        var milestonePhases = new[] { "Planning & Discovery", "Architecture Design", "Sprint 1 Implementation", "Sprint 2 Implementation", "QA & UAT", "Production Deployment", "Post-Launch Support" };
        
        foreach (var proj in projects)
        {
            int milestoneCount = faker.Random.Int(3, 5);
            var phases = faker.PickRandom(milestonePhases, milestoneCount).ToList();
            
            for (int i = 0; i < phases.Count; i++)
            {
                milestones.Add(new Milestone
                {
                    ProjectId = proj.Id,
                    Title = $"Phase {i + 1}: {phases[i]}",
                    DueDate = proj.StartDate.AddDays((i + 1) * 20),
                    Status = faker.PickRandom<MilestoneStatus>(),
                    StoryPoints = faker.Random.Int(10, 80)
                });
            }
            proj.TotalStoryPoints = milestones.Where(m => m.ProjectId == proj.Id).Sum(m => m.StoryPoints);
        }
        await context.Milestones.AddRangeAsync(milestones);
        await context.SaveChangesAsync();

        // 7. Seed Allocations
        var allocations = new List<Allocation>();
        foreach (var proj in projects)
        {
            var managerEmployees = employees.Where(e => e.ManagerId == proj.ManagerId).ToList();
            if (!managerEmployees.Any()) continue;
            
            var assignedEmployees = faker.PickRandom(managerEmployees, Math.Min(managerEmployees.Count, faker.Random.Int(2, 6))).ToList();
            foreach (var emp in assignedEmployees)
            {
                allocations.Add(new Allocation
                {
                    ProjectId = proj.Id,
                    UserId = emp.Id,
                    FromDate = proj.StartDate,
                    ToDate = proj.EndDate,
                    UtilisationPercent = faker.PickRandom(new[] { 50, 100 })
                });

                emp.Status = EmployeeStatus.Allocated;
            }
        }
        await context.Allocations.AddRangeAsync(allocations);
        await context.SaveChangesAsync();

        // 8. Seed Timesheets & Entries
        var timesheets = new List<Timesheet>();
        var timesheetEntries = new List<TimesheetEntry>();

        var userAllocations = allocations.GroupBy(a => a.UserId).Take(20).ToList();
        var weekStart = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek + (int)DayOfWeek.Monday).Date;

        foreach (var userGroup in userAllocations)
        {
            var ts = new Timesheet
            {
                UserId = userGroup.Key,
                WeekStartDate = weekStart.ToUniversalTime(),
                Status = TimesheetStatus.Submitted,
                SubmittedAt = DateTime.UtcNow
            };
            timesheets.Add(ts);
        }

        await context.Timesheets.AddRangeAsync(timesheets);
        await context.SaveChangesAsync();

        foreach (var ts in timesheets)
        {
            var userGroup = userAllocations.First(g => g.Key == ts.UserId);
            foreach (var allocation in userGroup)
            {
                timesheetEntries.Add(new TimesheetEntry
                {
                    TimesheetId = ts.Id,
                    ProjectId = allocation.ProjectId,
                    HoursWorked = allocation.UtilisationPercent == 100 ? 40m : 20m,
                    ActivityTags = faker.PickRandom(techTaskDescriptions)
                });
            }
        }
        await context.TimesheetEntries.AddRangeAsync(timesheetEntries);
        await context.SaveChangesAsync();
    }
}
