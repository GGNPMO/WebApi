using Microsoft.EntityFrameworkCore;
using Payroll.Domain.Entities;

namespace Payroll.Infrastructure.Persistence;

public class PayrollDbContext(DbContextOptions<PayrollDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<PayrollRecord> PayrollRecords => Set<PayrollRecord>();
    public DbSet<Deduction> Deductions => Set<Deduction>();
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.EmployeeCode).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.BaseSalary).HasColumnType("decimal(18,2)");
            e.Property(x => x.FirstName).HasMaxLength(100);
            e.Property(x => x.LastName).HasMaxLength(100);
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.Department).HasMaxLength(100);
            e.Property(x => x.Designation).HasMaxLength(100);
        });

        modelBuilder.Entity<PayrollRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.EmployeeId, x.Month, x.Year }).IsUnique();
            e.Property(x => x.BaseSalary).HasColumnType("decimal(18,2)");
            e.Property(x => x.HRA).HasColumnType("decimal(18,2)");
            e.Property(x => x.DA).HasColumnType("decimal(18,2)");
            e.Property(x => x.TA).HasColumnType("decimal(18,2)");
            e.Property(x => x.OtherAllowances).HasColumnType("decimal(18,2)");
            e.Property(x => x.GrossSalary).HasColumnType("decimal(18,2)");
            e.Property(x => x.TotalDeductions).HasColumnType("decimal(18,2)");
            e.Property(x => x.NetSalary).HasColumnType("decimal(18,2)");
            e.Property(x => x.TaxAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Status).HasMaxLength(20);
            e.HasOne(x => x.Employee).WithMany(x => x.PayrollRecords).HasForeignKey(x => x.EmployeeId);
        });

        modelBuilder.Entity<Deduction>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Type).HasMaxLength(50);
            e.Property(x => x.Description).HasMaxLength(200);
            e.HasOne(x => x.Employee).WithMany(x => x.Deductions).HasForeignKey(x => x.EmployeeId);
        });

        modelBuilder.Entity<AppUser>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Username).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Username).HasMaxLength(100);
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.Role).HasMaxLength(20);
        });
    }
}
