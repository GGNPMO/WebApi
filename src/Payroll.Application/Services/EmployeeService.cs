using Payroll.Application.Common;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Domain.Entities;
using Payroll.Domain.Interfaces;

namespace Payroll.Application.Services;

public class EmployeeService(IRepository<Employee> repository, IUnitOfWork unitOfWork) : IEmployeeService
{
    public async Task<ApiResponse<IReadOnlyList<EmployeeDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var employees = await repository.GetAllAsync(ct);
        var dtos = employees.Select(MapToDto).ToList().AsReadOnly();
        return ApiResponse<IReadOnlyList<EmployeeDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<EmployeeDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var emp = await repository.GetByIdAsync(id, ct);
        if (emp is null)
            return ApiResponse<EmployeeDto>.Fail("Employee not found");
        return ApiResponse<EmployeeDto>.Ok(MapToDto(emp));
    }

    public async Task<ApiResponse<EmployeeDto>> CreateAsync(CreateEmployeeRequest request, CancellationToken ct = default)
    {
        var emp = new Employee
        {
            EmployeeCode = $"EMP-{DateTime.UtcNow:yyyyMMddHHmmss}",
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Department = request.Department,
            Designation = request.Designation,
            DateOfJoining = request.DateOfJoining,
            BaseSalary = request.BaseSalary
        };

        await repository.AddAsync(emp, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return ApiResponse<EmployeeDto>.Ok(MapToDto(emp), "Employee created");
    }

    public async Task<ApiResponse<EmployeeDto>> UpdateAsync(int id, UpdateEmployeeRequest request, CancellationToken ct = default)
    {
        var emp = await repository.GetByIdAsync(id, ct);
        if (emp is null)
            return ApiResponse<EmployeeDto>.Fail("Employee not found");

        emp.FirstName = request.FirstName;
        emp.LastName = request.LastName;
        emp.Email = request.Email;
        emp.Phone = request.Phone;
        emp.Department = request.Department;
        emp.Designation = request.Designation;
        emp.BaseSalary = request.BaseSalary;
        emp.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(emp, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return ApiResponse<EmployeeDto>.Ok(MapToDto(emp), "Employee updated");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var emp = await repository.GetByIdAsync(id, ct);
        if (emp is null)
            return ApiResponse<bool>.Fail("Employee not found");

        emp.IsActive = false;
        emp.DateOfLeaving = DateTime.UtcNow;
        emp.UpdatedAt = DateTime.UtcNow;
        await repository.UpdateAsync(emp, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return ApiResponse<bool>.Ok(true, "Employee deactivated");
    }

    public async Task<ApiResponse<IReadOnlyList<EmployeeDto>>> SearchAsync(string term, CancellationToken ct = default)
    {
        var results = await repository.FindAsync(e =>
            e.FirstName.Contains(term) || e.LastName.Contains(term) ||
            e.Email.Contains(term) || e.EmployeeCode.Contains(term), ct);
        var dtos = results.Select(MapToDto).ToList().AsReadOnly();
        return ApiResponse<IReadOnlyList<EmployeeDto>>.Ok(dtos);
    }

    private static EmployeeDto MapToDto(Employee e) => new(
        e.Id, e.EmployeeCode, e.FirstName, e.LastName, e.Email,
        e.Phone, e.Department, e.Designation, e.DateOfJoining,
        e.BaseSalary, e.IsActive);
}
