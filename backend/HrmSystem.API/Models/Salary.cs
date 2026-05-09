namespace HrmSystem.API.Models;

public class Salary
{
	public int Id { get; set; }
	public int Month { get; set; }
	public int Year { get; set; }
	public int WorkDays { get; set; }
	public decimal Allowance { get; set; }
	public decimal Advance { get; set; }
	public decimal Total { get; set; }

	public int EmployeeId { get; set; }
	public Employee Employee { get; set; } = null!;
}