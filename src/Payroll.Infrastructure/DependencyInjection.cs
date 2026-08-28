using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payroll.Application.Interfaces;
using Payroll.Application.Services;
using Payroll.Domain.Interfaces;
using Payroll.Infrastructure.Auth;
using Payroll.Infrastructure.Persistence;

namespace Payroll.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<PayrollDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<IDeductionService, DeductionService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
