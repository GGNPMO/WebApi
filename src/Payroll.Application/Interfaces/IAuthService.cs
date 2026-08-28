using Payroll.Application.Common;
using Payroll.Application.DTOs;

namespace Payroll.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
}
