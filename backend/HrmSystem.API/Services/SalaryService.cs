using HrmSystem.API.Data;
using HrmSystem.API.Models;
using Microsoft.EntityFrameworkCore;
using HrmSystem.API.Hubs;          // ← THÊM DÒNG NÀY
using Microsoft.AspNetCore.SignalR; // ← THÊM DÒNG NÀY
namespace HrmSystem.API.Services;

public class SalaryService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public SalaryService(AppDbContext context,
        IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    // Lấy danh sách lương theo tháng
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

    // Tính lương tự động cho tất cả nhân viên trong tháng
    public async Task<string> CalculateSalaryAsync(int month, int year)
    {
        // Lấy tất cả nhân viên đang làm việc
        var employees = await _context.Employees
            .Include(e => e.Position)
            .Where(e => e.Status == "Active")
            .ToListAsync();

        int count = 0;

        foreach (var employee in employees)
        {
            // Kiểm tra đã tính lương chưa
            var existing = await _context.Salaries
                .FirstOrDefaultAsync(s =>
                    s.EmployeeId == employee.Id &&
                    s.Month == month &&
                    s.Year == year);

            if (existing != null) continue;

            // Đếm số ngày công trong tháng
            var workDays = await _context.Attendances
                .CountAsync(a =>
                    a.EmployeeId == employee.Id &&
                    a.Date.Month == month &&
                    a.Date.Year == year &&
                    a.CheckIn != null);

            // Lấy tổng tạm ứng trong tháng
            var advance = 0m;

            // Tính lương = (Lương cơ bản / 26 ngày) × số ngày công
            var dailySalary = employee.Position.BaseSalary / 26;
            var total = dailySalary * workDays - advance;

            var salary = new Salary
            {
                EmployeeId = employee.Id,
                Month = month,
                Year = year,
                WorkDays = workDays,
                Allowance = 0,
                Advance = advance,
                Total = total < 0 ? 0 : total
            };

            _context.Salaries.Add(salary);
            count++;
        }

// Gửi thông báo real-time cho tất cả nhân viên
await _hubContext.Clients.Group("Employee")
    .SendAsync("SalaryCalculated",
        $"Lương tháng {month}/{year} đã được tính xong!");

return $"Đã tính lương cho {count} nhân viên";
    }

    // Cập nhật lương thủ công
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

        // Tiêu đề
        sheet.Cell(1, 1).Value = $"BẢNG LƯƠNG THÁNG {month}/{year}";
        sheet.Range(1, 1, 1, 8).Merge().Style
            .Font.SetBold(true)
            .Font.SetFontSize(14)
            .Alignment.SetHorizontal(ClosedXML.Excel.XLAlignmentHorizontalValues.Center);

        // Header
        var headers = new[] {
        "STT", "Họ tên", "Phòng ban", "Chức vụ",
        "Ngày công", "Lương cơ bản", "Phụ cấp", "Tạm ứng", "Thực lĩnh"
    };
        for (int i = 0; i < headers.Length; i++)
        {
            sheet.Cell(3, i + 1).Value = headers[i];
            sheet.Cell(3, i + 1).Style
                .Font.SetBold(true)
                .Fill.SetBackgroundColor(ClosedXML.Excel.XLColor.LightBlue)
                .Alignment.SetHorizontal(ClosedXML.Excel.XLAlignmentHorizontalValues.Center);
        }

        // Dữ liệu
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

            // Format tiền tệ
            foreach (var col in new[] { 6, 7, 8, 9 })
                sheet.Cell(row, col).Style.NumberFormat.Format = "#,##0";
        }

        // Tổng cộng
        var lastRow = salaries.Count + 4;
        sheet.Cell(lastRow, 8).Value = "Tổng:";
        sheet.Cell(lastRow, 8).Style.Font.SetBold(true);
        sheet.Cell(lastRow, 9).FormulaA1 = $"=SUM(I4:I{lastRow - 1})";
        sheet.Cell(lastRow, 9).Style.Font.SetBold(true)
            .NumberFormat.Format = "#,##0";

        // Tự động độ rộng cột
        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}