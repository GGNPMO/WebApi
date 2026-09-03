using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payroll.Application.Common;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;


namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController(IEmployeeService employeeService) : ControllerBase
{
    [HttpGet]
    // Pagination:
    public async Task<IActionResult> GetAll([FromQuery] PaginationQuery pagination, CancellationToken ct) =>
        Ok(await employeeService.GetAllAsync(pagination, ct));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await employeeService.GetByIdAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("search")]
    // Pagination:
    public async Task<IActionResult> Search([FromQuery] string term, [FromQuery] PaginationQuery pagination, CancellationToken ct) =>
        Ok(await employeeService.SearchAsync(term, pagination, ct));

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Create(CreateEmployeeRequest request, CancellationToken ct)
    {
        var result = await employeeService.CreateAsync(request, ct);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Update(int id, UpdateEmployeeRequest request, CancellationToken ct)
    {
        var result = await employeeService.UpdateAsync(id, request, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await employeeService.DeleteAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
