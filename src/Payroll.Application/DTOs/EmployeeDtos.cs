namespace Payroll.Application.DTOs;

public record EmployeeDto(
    int Id,
    string EmployeeCode,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Department,
    string Designation,
    DateTime DateOfJoining,
    decimal BaseSalary,
    bool IsActive
);

public record CreateEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Department,
    string Designation,
    DateTime DateOfJoining,
    decimal BaseSalary
);

public record UpdateEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Department,
    string Designation,
    decimal BaseSalary
);
