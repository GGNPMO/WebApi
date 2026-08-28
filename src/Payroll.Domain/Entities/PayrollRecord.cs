namespace Payroll.Domain.Entities;

public class PayrollRecord
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal DA { get; set; }
    public decimal TA { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public decimal TaxAmount { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Processed, Paid
    public DateTime? ProcessedDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Employee Employee { get; set; } = null!;
}
