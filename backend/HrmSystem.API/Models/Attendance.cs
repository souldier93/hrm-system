namespace HrmSystem.API.Models;

public class Attendance
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly? CheckIn { get; set; }
    public TimeOnly? CheckOut { get; set; }
    public string Status { get; set; } = "OnTime"; // OnTime / Late / Absent
    public string? Notes { get; set; }

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
}
