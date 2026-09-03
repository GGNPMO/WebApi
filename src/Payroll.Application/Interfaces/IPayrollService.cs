using Payroll.Application.Common;
using Payroll.Application.DTOs;

namespace Payroll.Application.Interfaces;

public interface IPayrollService
{
    Task<ApiResponse<PayrollDto>> GeneratePayrollAsync(GeneratePayrollRequest request, CancellationToken ct = default);
    Task<ApiResponse<PayrollDto>> ProcessPayrollAsync(int payrollId, CancellationToken ct = default);
    Task<ApiResponse<IReadOnlyList<PayrollDto>>> GetByEmployeeAsync(int employeeId, CancellationToken ct = default);
    Task<ApiResponse<IReadOnlyList<PayrollDto>>> GetByMonthYearAsync(int month, int year, CancellationToken ct = default);
    Task<ApiResponse<PayrollDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ApiResponse<PayrollStatusDto>> GetStatusAsync(int id, CancellationToken ct = default);
}
