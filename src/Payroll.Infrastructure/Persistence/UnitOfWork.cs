using Payroll.Domain.Interfaces;

namespace Payroll.Infrastructure.Persistence;

public class UnitOfWork(PayrollDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);

    public void Dispose() => context.Dispose();
}
