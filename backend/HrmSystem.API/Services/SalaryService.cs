using HrmSystem.API.Data;
using HrmSystem.API.Models;
using Microsoft.EntityFrameworkCore;
using HrmSystem.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace HrmSystem.API.Services;

public class SalaryService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly NotificationService _notificationService;
    private readonly ILogger<SalaryService> _logger; // ← thêm

    public SalaryService(AppDbContext context,
        IHubContext<NotificationHub> hubContext,
        NotificationService notificationService,
        ILogger<SalaryService> logger) // ← thêm
    {
        _context = context;
        _hubContext = hubContext;
        _notificationService = notificationService;
        _logger = logger; // ← thêm
    }

    public async Task<List<object>> GetByMonthAsync(int month, int year)
    {
        return await _context.Salaries
            .Include(s => s.Employee)
                .ThenInclude(e => e.Position)
            .Where(s => s.Month == month && s.Year == year)
            .Select(s => (object)new
            {
                s.Id,
                s.Month,
                s.Year,
                s.WorkDays,
                s.Allowance,
                s.Advance,
                s.Total,
                EmployeeName = s.Employee.FullName,
                PositionName = s.Employee.Position.Name,
                BaseSalary = s.Employee.Position.BaseSalary,
                s.EmployeeId
            })
            .ToListAsync();
    }

    public async Task<string> CalculateSalaryAsync(int month, int year)
    {
        var employees = await _context.Employees
            .Include(e => e.Position)
            .Where(e => e.Status == "Active")
            .ToListAsync();

        int count = 0;

        foreach (var employee in employees)
        {
            var existing = await _context.Salaries
                .FirstOrDefaultAsync(s =>
                    s.EmployeeId == employee.Id &&
                    s.Month == month &&
                    s.Year == year);

            if (existing != null) continue;

            // Tính ngày đầu và cuối tháng
            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Đếm số ngày công trong tháng
            var workDays = await _context.Attendances
    .CountAsync(a =>
        a.EmployeeId == employee.Id &&
        a.Date >= startDate &&
        a.Date <= endDate &&
        a.CheckIn != null);

            _logger.LogInformation("EmployeeId={Id}, Month={Month}, WorkDays={Days}",
                employee.Id, month, workDays); // ← thêm dòng này

            var advance = 0m;
            var dailySalary = employee.Position.BaseSalary / 26;
            var total = dailySalary * workDays - advance;

            _context.Salaries.Add(new Salary
            {
                EmployeeId = employee.Id,
                Month = month,
                Year = year,
                WorkDays = workDays,
                Allowance = 0,
                Advance = advance,
                Total = total < 0 ? 0 : total
            });
            count++;
        }

        await _context.SaveChangesAsync();

        // Gửi thông báo đến từng nhân viên
        foreach (var employee in employees)
        {
            await _notificationService.SendToEmployeeAsync(
                employee.Id,
                "💰 Lương đã được tính",
                $"Lương tháng {month}/{year} của bạn đã được tính xong!",
                "Success"
            );
        }

        return $"Đã tính lương cho {count} nhân viên";
    }

    public async Task<bool> UpdateAsync(int id, decimal allowance,
        decimal advance, decimal total)
    {
        var salary = await _context.Salaries.FindAsync(id);
        if (salary == null) return false;

        salary.Allowance = allowance;
        salary.Advance = advance;
        salary.Total = total;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<byte[]> ExportExcelAsync(int month, int year)
    {
        var salaries = await _context.Salaries
            .Include(s => s.Employee)
                .ThenInclude(e => e.Position)
            .Include(s => s.Employee)
                .ThenInclude(e => e.Department)
            .Where(s => s.Month == month && s.Year == year)
            .ToListAsync();

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var sheet = workbook.Worksheets.Add($"Lương T{month}-{year}");

        sheet.Cell(1, 1).Value = $"BẢNG LƯƠNG THÁNG {month}/{year}";
        sheet.Range(1, 1, 1, 9).Merge().Style
            .Font.SetBold(true)
            .Font.SetFontSize(14)
            .Alignment.SetHorizontal(
                ClosedXML.Excel.XLAlignmentHorizontalValues.Center);

        var headers = new[] {
            "STT","Họ tên","Phòng ban","Chức vụ",
            "Ngày công","Lương cơ bản","Phụ cấp","Tạm ứng","Thực lĩnh"
        };
        for (int i = 0; i < headers.Length; i++)
        {
            sheet.Cell(3, i + 1).Value = headers[i];
            sheet.Cell(3, i + 1).Style
                .Font.SetBold(true)
                .Fill.SetBackgroundColor(ClosedXML.Excel.XLColor.LightBlue)
                .Alignment.SetHorizontal(
                    ClosedXML.Excel.XLAlignmentHorizontalValues.Center);
        }

        for (int i = 0; i < salaries.Count; i++)
        {
            var s = salaries[i];
            var row = i + 4;
            sheet.Cell(row, 1).Value = i + 1;
            sheet.Cell(row, 2).Value = s.Employee.FullName;
            sheet.Cell(row, 3).Value = s.Employee.Department.Name;
            sheet.Cell(row, 4).Value = s.Employee.Position.Name;
            sheet.Cell(row, 5).Value = s.WorkDays;
            sheet.Cell(row, 6).Value = (double)s.Employee.Position.BaseSalary;
            sheet.Cell(row, 7).Value = (double)s.Allowance;
            sheet.Cell(row, 8).Value = (double)s.Advance;
            sheet.Cell(row, 9).Value = (double)s.Total;

            foreach (var col in new[] { 6, 7, 8, 9 })
                sheet.Cell(row, col).Style.NumberFormat.Format = "#,##0";
        }

        var lastRow = salaries.Count + 4;
        sheet.Cell(lastRow, 8).Value = "Tổng:";
        sheet.Cell(lastRow, 8).Style.Font.SetBold(true);
        sheet.Cell(lastRow, 9).FormulaA1 = $"=SUM(I4:I{lastRow - 1})";
        sheet.Cell(lastRow, 9).Style.Font.SetBold(true)
            .NumberFormat.Format = "#,##0";

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}