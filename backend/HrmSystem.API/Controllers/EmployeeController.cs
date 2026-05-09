using HrmSystem.API.DTOs;
using HrmSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Phải đăng nhập mới dùng được
public class EmployeeController : ControllerBase
{
	private readonly EmployeeService _employeeService;

	public EmployeeController(EmployeeService employeeService)
	{
		_employeeService = employeeService;
	}

	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		var result = await _employeeService.GetAllAsync();
		return Ok(result);
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetById(int id)
	{
		var result = await _employeeService.GetByIdAsync(id);
		if (result == null) return NotFound();
		return Ok(result);
	}

	[HttpPost]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Create(EmployeeRequest request)
	{
		var result = await _employeeService.CreateAsync(request);
		return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
	}

	[HttpPut("{id}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Update(int id, EmployeeRequest request)
	{
		var result = await _employeeService.UpdateAsync(id, request);
		if (result == null) return NotFound();
		return Ok(result);
	}

	[HttpDelete("{id}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Delete(int id)
	{
		var success = await _employeeService.DeleteAsync(id);
		if (!success) return NotFound();
		return NoContent();
	}
}