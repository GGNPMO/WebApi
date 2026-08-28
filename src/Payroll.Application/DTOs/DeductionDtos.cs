namespace Payroll.Application.DTOs;

public record DeductionDto(
    int Id,
    int EmployeeId,
    string Type,
    string Description,
    decimal Amount,
    bool IsPercentage,
    bool IsActive,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo
);

public record CreateDeductionRequest(
    int EmployeeId,
    string Type,
    string Description,
    decimal Amount,
    bool IsPercentage,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo
);
