using HrmSystem.API.Data;
using HrmSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HrmSystem.API.Services;

public class AttendanceService
{
    private readonly AppDbContext _context;

    public AttendanceService(AppDbContext context)
    {
        _context = context;
    }

    // Lấy danh sách chấm công theo tháng
    public async Task<List<object>> GetByMonthAsync(int month, int year)
    {
        return await _context.Attendances
            .Include(a => a.Employee)
            .Where(a => a.Date.Month == month && a.Date.Year == year)
            .OrderByDescending(a => a.Date)
            .Select(a => (object)new
            {
                a.Id,
                a.Date,
                a.CheckIn,
                a.CheckOut,
                a.Status,
                a.Notes,
                EmployeeName = a.Employee.FullName,
                a.EmployeeId
            })
            .ToListAsync();
    }

    // Lấy chấm công hôm nay của nhân viên
    public async Task<object?> GetTodayAsync(int employeeId)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var attendance = await _context.Attendances
            .FirstOrDefaultAsync(a =>
                a.EmployeeId == employeeId &&
                a.Date == today);

        if (attendance == null) return null;

        return new
        {
            attendance.Id,
            attendance.Date,
            attendance.CheckIn,
            attendance.CheckOut,
            attendance.Status,
            attendance.Notes
        };
    }

    // Check in
    public async Task<object> CheckInAsync(int employeeId)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var existing = await _context.Attendances
            .FirstOrDefaultAsync(a =>
                a.EmployeeId == employeeId &&
                a.Date == today);

        if (existing != null)
            throw new Exception("Hôm nay bạn đã check in rồi!");

        var now = DateTime.Now;
        var attendance = new Attendance
        {
            EmployeeId = employeeId,
            Date = today,
            CheckIn = TimeOnly.FromDateTime(now),
            // Sau 8:15 là muộn
            Status = (now.Hour > 8 || (now.Hour == 8 && now.Minute > 15))
                     ? "Late" : "OnTime"
        };

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();

        return new
        {
            attendance.Id,
            attendance.Date,
            attendance.CheckIn,
            attendance.Status
        };
    }

    // Check out
    public async Task<object> CheckOutAsync(int employeeId)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var attendance = await _context.Attendances
            .FirstOrDefaultAsync(a =>
                a.EmployeeId == employeeId &&
                a.Date == today);

        if (attendance == null)
            throw new Exception("Bạn chưa check in hôm nay!");

        if (attendance.CheckOut != null)
            throw new Exception("Bạn đã check out rồi!");

        attendance.CheckOut = TimeOnly.FromDateTime(DateTime.Now);
        await _context.SaveChangesAsync();

        return new
        {
            attendance.Id,
            attendance.CheckIn,
            attendance.CheckOut,
            attendance.Status
        };
    }

    // Admin chỉnh sửa chấm công
    public async Task<bool> UpdateAsync(int id, DateOnly date,
        TimeOnly? checkIn, TimeOnly? checkOut, string status, string? notes)
    {
        var attendance = await _context.Attendances.FindAsync(id);
        if (attendance == null) return false;

        attendance.Date = date;
        attendance.CheckIn = checkIn;
        attendance.CheckOut = checkOut;
        attendance.Status = status;
        attendance.Notes = notes;

        await _context.SaveChangesAsync();
        return true;
    }
}