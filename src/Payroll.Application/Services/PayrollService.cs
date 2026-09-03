using Payroll.Application.Common;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Domain.Entities;
using Payroll.Domain.Interfaces;

namespace Payroll.Application.Services;

public class PayrollService(
    IRepository<PayrollRecord> payrollRepo,
    IRepository<Employee> employeeRepo,
    IRepository<Deduction> deductionRepo,
    IUnitOfWork unitOfWork) : IPayrollService
{
    public async Task<ApiResponse<PayrollDto>> GeneratePayrollAsync(GeneratePayrollRequest request, CancellationToken ct = default)
    {
        var emp = await employeeRepo.GetByIdAsync(request.EmployeeId, ct);
        if (emp is null)
            return ApiResponse<PayrollDto>.Fail("Employee not found");

        var existing = await payrollRepo.FindAsync(p =>
            p.EmployeeId == request.EmployeeId && p.Month == request.Month && p.Year == request.Year, ct);
        if (existing.Count > 0)
            return ApiResponse<PayrollDto>.Fail("Payroll already exists for this month");

        var deductions = await deductionRepo.FindAsync(d =>
            d.EmployeeId == request.EmployeeId && d.IsActive &&
            d.EffectiveFrom <= DateTime.UtcNow &&
            (d.EffectiveTo == null || d.EffectiveTo >= DateTime.UtcNow), ct);

        // Salary calculation
        var hra = emp.BaseSalary * 0.40m;
        var da = emp.BaseSalary * 0.12m;
        var ta = emp.BaseSalary * 0.10m;
        var otherAllowances = emp.BaseSalary * 0.08m;
        var gross = emp.BaseSalary + hra + da + ta + otherAllowances;

        var totalDeductions = deductions.Sum(d =>
            d.IsPercentage ? gross * (d.Amount / 100m) : d.Amount);

        // Simple tax slab (India-style for demo)
        var annualGross = gross * 12;
        var tax = annualGross switch
        {
            <= 500000m => 0m,
            <= 1000000m => (annualGross - 500000m) * 0.20m / 12m,
            _ => ((500000m * 0.20m) + (annualGross - 1000000m) * 0.30m) / 12m
        };

        var record = new PayrollRecord
        {
            EmployeeId = request.EmployeeId,
            Month = request.Month,
            Year = request.Year,
            BaseSalary = emp.BaseSalary,
            HRA = hra,
            DA = da,
            TA = ta,
            OtherAllowances = otherAllowances,
            GrossSalary = gross,
            TotalDeductions = totalDeductions,
            TaxAmount = Math.Round(tax, 2),
            NetSalary = gross - totalDeductions - Math.Round(tax, 2),
            Status = "Draft"
        };

        await payrollRepo.AddAsync(record, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return ApiResponse<PayrollDto>.Ok(MapToDto(record, emp.FullName), "Payroll generated");
    }

    public async Task<ApiResponse<PayrollDto>> ProcessPayrollAsync(int payrollId, CancellationToken ct = default)
    {
        var record = await payrollRepo.GetByIdAsync(payrollId, ct);
        if (record is null)
            return ApiResponse<PayrollDto>.Fail("Payroll record not found");

        record.Status = "Processed";
        record.ProcessedDate = DateTime.UtcNow;
        await payrollRepo.UpdateAsync(record, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var emp = await employeeRepo.GetByIdAsync(record.EmployeeId, ct);
        return ApiResponse<PayrollDto>.Ok(MapToDto(record, emp?.FullName ?? ""), "Payroll processed");
    }

    public async Task<ApiResponse<IReadOnlyList<PayrollDto>>> GetByEmployeeAsync(int employeeId, CancellationToken ct = default)
    {
        var records = await payrollRepo.FindAsync(p => p.EmployeeId == employeeId, ct);
        var emp = await employeeRepo.GetByIdAsync(employeeId, ct);
        var dtos = records.Select(r => MapToDto(r, emp?.FullName ?? "")).ToList().AsReadOnly();
        return ApiResponse<IReadOnlyList<PayrollDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<IReadOnlyList<PayrollDto>>> GetByMonthYearAsync(int month, int year, CancellationToken ct = default)
    {
        var records = await payrollRepo.FindAsync(p => p.Month == month && p.Year == year, ct);
        var dtos = new List<PayrollDto>();
        foreach (var r in records)
        {
            var emp = await employeeRepo.GetByIdAsync(r.EmployeeId, ct);
            dtos.Add(MapToDto(r, emp?.FullName ?? ""));
        }
        return ApiResponse<IReadOnlyList<PayrollDto>>.Ok(dtos.AsReadOnly());
    }

    public async Task<ApiResponse<PayrollDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var record = await payrollRepo.GetByIdAsync(id, ct);
        if (record is null)
            return ApiResponse<PayrollDto>.Fail("Payroll record not found");
        var emp = await employeeRepo.GetByIdAsync(record.EmployeeId, ct);
        return ApiResponse<PayrollDto>.Ok(MapToDto(record, emp?.FullName ?? ""));
    }

    public async Task<ApiResponse<PayrollStatusDto>> GetStatusAsync(int id, CancellationToken ct = default)
    {
        // Polling reads the latest persisted state without loading payroll details.
        var record = await payrollRepo.GetByIdAsync(id, ct);
        if (record is null)
            return ApiResponse<PayrollStatusDto>.Fail("Payroll record not found");

        return ApiResponse<PayrollStatusDto>.Ok(
            new PayrollStatusDto(record.Id, record.Status, record.ProcessedDate));
    }

    private static PayrollDto MapToDto(PayrollRecord r, string empName) => new(
        r.Id, r.EmployeeId, empName, r.Month, r.Year,
        r.BaseSalary, r.HRA, r.DA, r.TA, r.OtherAllowances,
        r.GrossSalary, r.TotalDeductions, r.NetSalary, r.TaxAmount,
        r.Status, r.ProcessedDate);
}
