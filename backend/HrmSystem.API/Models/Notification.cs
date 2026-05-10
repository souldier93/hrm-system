namespace HrmSystem.API.Models;

public class Notification
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string Type { get; set; } = "Info"; // Info / Success / Warning
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
}
