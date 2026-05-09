namespace HrmSystem.API.Models;

public class Position
{
	public int Id { get; set; }
	public string Name { get; set; } = null!;
	public decimal BaseSalary { get; set; }
	public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}