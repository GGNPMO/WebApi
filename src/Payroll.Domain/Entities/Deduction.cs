namespace Payroll.Domain.Entities;

public class Deduction
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string Type { get; set; } = string.Empty; // PF, ESI, Tax, LoanEMI, Insurance
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsPercentage { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    public Employee Employee { get; set; } = null!;
}
