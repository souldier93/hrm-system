using HrmSystem.API.Data;
using HrmSystem.API.DTOs;
using HrmSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HrmSystem.API.Services;

public class EmployeeService
{
    private readonly AppDbContext _context;
    private readonly CacheService _cache;
    private const string CacheKey = "employees:all";

    public EmployeeService(AppDbContext context, CacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<EmployeeResponse>> GetAllAsync()
    {
        // Thử lấy từ cache trước
        var cached = await _cache.GetAsync<List<EmployeeResponse>>(CacheKey);
        if (cached != null) return cached;

        // Không có trong cache → query database
        var result = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Select(e => new EmployeeResponse
            {
                Id = e.Id,
                FullName = e.FullName,
                Phone = e.Phone,
                Email = e.Email,
                JoinDate = e.JoinDate,
                Status = e.Status,
                DepartmentName = e.Department.Name,
                PositionName = e.Position.Name
            })
            .ToListAsync();

        // Lưu vào cache 5 phút
        await _cache.SetAsync(CacheKey, result, TimeSpan.FromMinutes(5));
        return result;
    }

    public async Task<EmployeeResponse?> GetByIdAsync(int id)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Where(e => e.Id == id)
            .Select(e => new EmployeeResponse
            {
                Id = e.Id,
                FullName = e.FullName,
                Phone = e.Phone,
                Email = e.Email,
                JoinDate = e.JoinDate,
                Status = e.Status,
                DepartmentName = e.Department.Name,
                PositionName = e.Position.Name
            })
            .FirstOrDefaultAsync();
    }

    public async Task<EmployeeResponse> CreateAsync(EmployeeRequest request)
    {
        var employee = new Employee
        {
            FullName = request.FullName,
            Phone = request.Phone,
            Email = request.Email,
            JoinDate = request.JoinDate,
            DepartmentId = request.DepartmentId,
            PositionId = request.PositionId,
            Status = "Active"
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Xóa cache vì có dữ liệu mới
        await _cache.RemoveAsync(CacheKey);

        return (await GetByIdAsync(employee.Id))!;
    }

    public async Task<EmployeeResponse?> UpdateAsync(int id, EmployeeRequest request)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return null;

        employee.FullName = request.FullName;
        employee.Phone = request.Phone;
        employee.Email = request.Email;
        employee.JoinDate = request.JoinDate;
        employee.DepartmentId = request.DepartmentId;
        employee.PositionId = request.PositionId;

        await _context.SaveChangesAsync();

        // Xóa cache vì có thay đổi
        await _cache.RemoveAsync(CacheKey);

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return false;

        employee.Status = "Inactive";
        await _context.SaveChangesAsync();

        // Xóa cache vì có thay đổi
        await _cache.RemoveAsync(CacheKey);

        return true;
    }
}