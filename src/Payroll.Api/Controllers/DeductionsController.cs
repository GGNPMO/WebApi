using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payroll.Application.Common;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,HR")]
public class DeductionsController(IDeductionService deductionService) : ControllerBase
{
    [HttpGet("employee/{employeeId}")]
    // Pagination:
    public async Task<IActionResult> GetByEmployee(int employeeId, [FromQuery] PaginationQuery pagination, CancellationToken ct) =>
        Ok(await deductionService.GetByEmployeeAsync(employeeId, pagination, ct));

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
