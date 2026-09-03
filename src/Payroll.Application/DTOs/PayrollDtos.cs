namespace Payroll.Application.DTOs;

public record PayrollDto(
    int Id,
    int EmployeeId,
    string EmployeeName,
    int Month,
    int Year,
    decimal BaseSalary,
    decimal HRA,
    decimal DA,
    decimal TA,
    decimal OtherAllowances,
    decimal GrossSalary,
    decimal TotalDeductions,
    decimal NetSalary,
    decimal TaxAmount,
    string Status,
    DateTime? ProcessedDate
);

public record PayrollStatusDto(
    int Id,
    string Status,
    DateTime? ProcessedDate
);

public record GeneratePayrollRequest(int EmployeeId, int Month, int Year);

public record ProcessPayrollRequest(int PayrollId);

public record PayrollQuery(
    int? EmployeeId = null,
    int? Month = null,
    int? Year = null,
    string? Status = null,
    string SortBy = "createdAt",
    bool SortDescending = true
);
