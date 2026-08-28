using Payroll.Application.Common;
using Payroll.Application.DTOs;

namespace Payroll.Application.Interfaces;

public interface IEmployeeService
{
    Task<ApiResponse<IReadOnlyList<EmployeeDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<EmployeeDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ApiResponse<EmployeeDto>> CreateAsync(CreateEmployeeRequest request, CancellationToken ct = default);
    Task<ApiResponse<EmployeeDto>> UpdateAsync(int id, UpdateEmployeeRequest request, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken ct = default);
    Task<ApiResponse<IReadOnlyList<EmployeeDto>>> SearchAsync(string term, CancellationToken ct = default);
}
