using Microsoft.EntityFrameworkCore;
using PRM.Core.Entities;
using PRM.Core.Interfaces;
using PRM.Infrastructure.Data;

namespace PRM.Infrastructure.Repositories;

public class TimesheetRepository : ITimesheetRepository
{
    private readonly PrmDbContext _context;

    public TimesheetRepository(PrmDbContext context)
    {
        _context = context;
    }

    public async Task<Timesheet?> GetByIdAsync(int id)
    {
        return await _context.Timesheets
            .Include(t => t.User)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Project)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Timesheet?> GetByEmployeeAndWeekAsync(int userId, DateTime weekStartDate)
    {
        var targetDate = weekStartDate.Date;
        return await _context.Timesheets
            .Include(t => t.Entries)
            .FirstOrDefaultAsync(t => t.UserId == userId && t.WeekStartDate.Date == targetDate);
    }

    public async Task<IReadOnlyList<Timesheet>> GetByEmployeeIdAsync(int userId)
    {
        return await _context.Timesheets
            .Include(t => t.Entries)
                .ThenInclude(e => e.Project)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.WeekStartDate)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Timesheet>> GetTeamTimesheetsAsync(int managerId, DateTime weekStartDate)
    {
        var targetDate = weekStartDate.Date;
        return await _context.Timesheets
            .Include(t => t.User)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Project)
            .Where(t => t.User.ManagerId == managerId && t.WeekStartDate.Date == targetDate)
            .OrderBy(t => t.User.FullName)
            .ToListAsync();
    }

    public async Task AddAsync(Timesheet timesheet)
    {
        await _context.Timesheets.AddAsync(timesheet);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Timesheet timesheet)
    {
        _context.Timesheets.Update(timesheet);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<TimesheetEntry>> GetRecentEntriesByProjectAsync(int projectId, int limitWeeks)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-limitWeeks * 7).Date;
        return await _context.TimesheetEntries
            .Include(e => e.Timesheet)
            .Where(e => e.ProjectId == projectId && e.Timesheet.WeekStartDate >= cutoffDate)
            .OrderByDescending(e => e.Timesheet.WeekStartDate)
            .ToListAsync();
    }
}
