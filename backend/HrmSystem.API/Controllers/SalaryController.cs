using HrmSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalaryController : ControllerBase
{
    private readonly SalaryService _salaryService;

    public SalaryController(SalaryService salaryService)
    {
        _salaryService = salaryService;
    }

    // Lấy danh sách lương theo tháng
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetByMonth(
        [FromQuery] int month, [FromQuery] int year)
    {
        if (month == 0) month = DateTime.Now.Month;
        if (year == 0) year = DateTime.Now.Year;
        var result = await _salaryService.GetByMonthAsync(month, year);
        return Ok(result);
    }

    // Tính lương tự động
    [HttpPost("calculate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Calculate(
        [FromQuery] int month, [FromQuery] int year)
    {
        if (month == 0) month = DateTime.Now.Month;
        if (year == 0) year = DateTime.Now.Year;
        var message = await _salaryService.CalculateSalaryAsync(month, year);
        return Ok(new { message });
    }

    // Cập nhật lương
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id,
        [FromBody] UpdateSalaryRequest req)
    {
        var success = await _salaryService.UpdateAsync(
            id, req.Allowance, req.Advance, req.Total);
        if (!success) return NotFound();
        return Ok();
    }

    [HttpGet("export")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Export(
    [FromQuery] int month, [FromQuery] int year)
    {
        if (month == 0) month = DateTime.Now.Month;
        if (year == 0) year = DateTime.Now.Year;

        var fileBytes = await _salaryService.ExportExcelAsync(month, year);
        return File(fileBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"BangLuong_T{month}_{year}.xlsx");
    }
}

public class UpdateSalaryRequest
{
    public decimal Allowance { get; set; }
    public decimal Advance { get; set; }
    public decimal Total { get; set; }
}