using HrmSystem.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HrmSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    // Tổng quan số liệu
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var currentMonth = today.Month;
        var currentYear = today.Year;

        var totalEmployees = await _context.Employees
            .CountAsync(e => e.Status == "Active");

        var checkedInToday = await _context.Attendances
            .CountAsync(a => a.Date == today && a.CheckIn != null);

        var lateToday = await _context.Attendances
            .CountAsync(a => a.Date == today && a.Status == "Late");

        var totalSalaryThisMonth = await _context.Salaries
            .Where(s => s.Month == currentMonth && s.Year == currentYear)
            .SumAsync(s => s.Total);

        //var pendingLeaves = await _context.LeaveRequests
        //    .CountAsync(l => l.Status == "Pending");

        return Ok(new
        {
            totalEmployees,
            checkedInToday,
            lateToday,
            totalSalaryThisMonth,
            //pendingLeaves
        });
    }

    // Nhân viên theo phòng ban (Pie chart)
    [HttpGet("employees-by-department")]
    public async Task<IActionResult> GetEmployeesByDepartment()
    {
        var data = await _context.Employees
            .Where(e => e.Status == "Active")
            .GroupBy(e => e.Department.Name)
            .Select(g => new
            {
                department = g.Key,
                count = g.Count()
            })
            .ToListAsync();

        return Ok(data);
    }

    // Chấm công theo tháng (Bar chart)
    [HttpGet("attendance-by-month")]
    public async Task<IActionResult> GetAttendanceByMonth([FromQuery] int year)
    {
        if (year == 0) year = DateTime.Now.Year;

        var data = await _context.Attendances
            .Where(a => a.Date.Year == year)
            .GroupBy(a => a.Date.Month)
            .Select(g => new
            {
                month = g.Key,
                onTime = g.Count(a => a.Status == "OnTime"),
                late = g.Count(a => a.Status == "Late"),
                absent = g.Count(a => a.Status == "Absent")
            })
            .OrderBy(x => x.month)
            .ToListAsync();

        return Ok(data);
    }

    // Tổng lương theo tháng (Line chart)
    [HttpGet("salary-by-month")]
    public async Task<IActionResult> GetSalaryByMonth([FromQuery] int year)
    {
        if (year == 0) year = DateTime.Now.Year;

        var data = await _context.Salaries
            .Where(s => s.Year == year)
            .GroupBy(s => s.Month)
            .Select(g => new
            {
                month = g.Key,
                total = g.Sum(s => s.Total)
            })
            .OrderBy(x => x.month)
            .ToListAsync();

        return Ok(data);
    }

    // Top 5 nhân viên chuyên cần nhất tháng
    [HttpGet("top-attendance")]
    public async Task<IActionResult> GetTopAttendance(
        [FromQuery] int month, [FromQuery] int year)
    {
        if (month == 0) month = DateTime.Now.Month;
        if (year == 0) year = DateTime.Now.Year;

        var data = await _context.Attendances
            .Where(a => a.Date.Month == month &&
                        a.Date.Year == year &&
                        a.CheckIn != null)
            .GroupBy(a => a.Employee.FullName)
            .Select(g => new
            {
                name = g.Key,
                workDays = g.Count()
            })
            .OrderByDescending(x => x.workDays)
            .Take(5)
            .ToListAsync();

        return Ok(data);
    }
}