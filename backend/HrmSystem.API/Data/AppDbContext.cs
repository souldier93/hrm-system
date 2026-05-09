using HrmSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HrmSystem.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Department> Departments { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<Salary> Salaries { get; set; }
}