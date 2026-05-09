namespace HrmSystem.API.DTOs;

public class EmployeeRequest
{
    public string FullName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateOnly JoinDate { get; set; }
    public int DepartmentId { get; set; }
    public int PositionId { get; set; }
}

public class EmployeeResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateOnly JoinDate { get; set; }
    public string Status { get; set; } = null!;
    public string DepartmentName { get; set; } = null!;
    public string PositionName { get; set; } = null!;
}
