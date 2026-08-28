using Payroll.Application.Common;
using Payroll.Application.DTOs;

namespace Payroll.Application.Interfaces;

public interface IDeductionService
{
    Task<ApiResponse<IReadOnlyList<DeductionDto>>> GetByEmployeeAsync(int employeeId, CancellationToken ct = default);
    Task<ApiResponse<DeductionDto>> CreateAsync(CreateDeductionRequest request, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken ct = default);
}
