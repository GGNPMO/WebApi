using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,HR")]
public class PayrollController(IPayrollService payrollService) : ControllerBase
{
    [HttpPost("generate")]
    public async Task<IActionResult> Generate(GeneratePayrollRequest request, CancellationToken ct)
    {
        var result = await payrollService.GeneratePayrollAsync(request, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/process")]
    public async Task<IActionResult> Process(int id, CancellationToken ct)
    {
        var result = await payrollService.ProcessPayrollAsync(id, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("employee/{employeeId}")]
    [Authorize]
    public async Task<IActionResult> GetByEmployee(int employeeId, CancellationToken ct) =>
        Ok(await payrollService.GetByEmployeeAsync(employeeId, ct));

    [HttpGet("period")]
    public async Task<IActionResult> GetByPeriod([FromQuery] int month, [FromQuery] int year, CancellationToken ct) =>
        Ok(await payrollService.GetByMonthYearAsync(month, year, ct));

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await payrollService.GetByIdAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
