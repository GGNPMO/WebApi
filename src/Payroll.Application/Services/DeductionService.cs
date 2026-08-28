using Payroll.Application.Common;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Domain.Entities;
using Payroll.Domain.Interfaces;

namespace Payroll.Application.Services;

public class DeductionService(IRepository<Deduction> repository, IUnitOfWork unitOfWork) : IDeductionService
{
    public async Task<ApiResponse<IReadOnlyList<DeductionDto>>> GetByEmployeeAsync(int employeeId, CancellationToken ct = default)
    {
        var deductions = await repository.FindAsync(d => d.EmployeeId == employeeId, ct);
        var dtos = deductions.Select(MapToDto).ToList().AsReadOnly();
        return ApiResponse<IReadOnlyList<DeductionDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<DeductionDto>> CreateAsync(CreateDeductionRequest request, CancellationToken ct = default)
    {
        var deduction = new Deduction
        {
            EmployeeId = request.EmployeeId,
            Type = request.Type,
            Description = request.Description,
            Amount = request.Amount,
            IsPercentage = request.IsPercentage,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo
        };

        await repository.AddAsync(deduction, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return ApiResponse<DeductionDto>.Ok(MapToDto(deduction), "Deduction created");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var deduction = await repository.GetByIdAsync(id, ct);
        if (deduction is null)
            return ApiResponse<bool>.Fail("Deduction not found");

        deduction.IsActive = false;
        await repository.UpdateAsync(deduction, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return ApiResponse<bool>.Ok(true, "Deduction deactivated");
    }

    private static DeductionDto MapToDto(Deduction d) => new(
        d.Id, d.EmployeeId, d.Type, d.Description,
        d.Amount, d.IsPercentage, d.IsActive,
        d.EffectiveFrom, d.EffectiveTo);
}
