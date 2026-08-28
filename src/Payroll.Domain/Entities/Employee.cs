namespace Payroll.Domain.Entities;

public class Employee
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public DateTime DateOfJoining { get; set; }
    public DateTime? DateOfLeaving { get; set; }
    public decimal BaseSalary { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public ICollection<PayrollRecord> PayrollRecords { get; set; } = [];
    public ICollection<Deduction> Deductions { get; set; } = [];
}
