using Payroll.Application.Common;
using Payroll.Application.DTOs;

namespace Payroll.Application.Interfaces;

public interface IEmployeeService
{
    // Pagination:
    Task<ApiResponse<PagedResult<EmployeeDto>>> GetAllAsync(PaginationQuery pagination, CancellationToken ct = default);
    Task<ApiResponse<EmployeeDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ApiResponse<EmployeeDto>> CreateAsync(CreateEmployeeRequest request, CancellationToken ct = default);
    Task<ApiResponse<EmployeeDto>> UpdateAsync(int id, UpdateEmployeeRequest request, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken ct = default);
    // Pagination:
    Task<ApiResponse<PagedResult<EmployeeDto>>> SearchAsync(string term, PaginationQuery pagination, CancellationToken ct = default);
}
