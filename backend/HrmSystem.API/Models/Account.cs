namespace HrmSystem.API.Models;

public class Account
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = "Employee"; // Admin / Manager / Employee

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
}