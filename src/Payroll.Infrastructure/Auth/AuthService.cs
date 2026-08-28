using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Payroll.Application.Common;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Domain.Entities;
using Payroll.Infrastructure.Persistence;

namespace Payroll.Infrastructure.Auth;

public class AuthService(PayrollDbContext context, IConfiguration config) : IAuthService
{
    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (await context.Users.AnyAsync(u => u.Username == request.Username, ct))
            return ApiResponse<AuthResponse>.Fail("Username already exists");

        if (await context.Users.AnyAsync(u => u.Email == request.Email, ct))
            return ApiResponse<AuthResponse>.Fail("Email already exists");

        var user = new AppUser
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCryptHash(request.Password),
            Role = request.Role
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(ct);

        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await context.SaveChangesAsync(ct);

        return ApiResponse<AuthResponse>.Ok(new AuthResponse(
            token, refreshToken, DateTime.UtcNow.AddHours(1), user.Username, user.Role), "Registration successful");
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive, ct);
        if (user is null || !BCryptVerify(request.Password, user.PasswordHash))
            return ApiResponse<AuthResponse>.Fail("Invalid username or password");

        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await context.SaveChangesAsync(ct);

        return ApiResponse<AuthResponse>.Ok(new AuthResponse(
            token, refreshToken, DateTime.UtcNow.AddHours(1), user.Username, user.Role));
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        var principal = GetPrincipalFromExpiredToken(request.Token);
        if (principal is null)
            return ApiResponse<AuthResponse>.Fail("Invalid token");

        var username = principal.Identity?.Name;
        var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

        if (user is null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiry < DateTime.UtcNow)
            return ApiResponse<AuthResponse>.Fail("Invalid or expired refresh token");

        var newToken = GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await context.SaveChangesAsync(ct);

        return ApiResponse<AuthResponse>.Ok(new AuthResponse(
            newToken, newRefreshToken, DateTime.UtcNow.AddHours(1), user.Username, user.Role));
    }

    private string GenerateJwtToken(AppUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("userId", user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var validation = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
        };

        var handler = new JwtSecurityTokenHandler();
        try
        {
            return handler.ValidateToken(token, validation, out _);
        }
        catch
        {
            return null;
        }
    }

    // Simple HMAC-based hash for demo — use BCrypt NuGet package in production
    private static string BCryptHash(string password)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("PayrollSecretSalt2026"));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hash);
    }

    private static bool BCryptVerify(string password, string storedHash) =>
        BCryptHash(password) == storedHash;
}
