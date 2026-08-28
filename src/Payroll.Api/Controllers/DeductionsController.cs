using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,HR")]
public class DeductionsController(IDeductionService deductionService) : ControllerBase
{
    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetByEmployee(int employeeId, CancellationToken ct) =>
        Ok(await deductionService.GetByEmployeeAsync(employeeId, ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateDeductionRequest request, CancellationToken ct)
    {
        var result = await deductionService.CreateAsync(request, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await deductionService.DeleteAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
