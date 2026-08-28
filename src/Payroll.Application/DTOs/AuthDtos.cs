namespace Payroll.Application.DTOs;

public record LoginRequest(string Username, string Password);

public record RegisterRequest(string Username, string Email, string Password, string Role);

public record AuthResponse(string Token, string RefreshToken, DateTime Expiration, string Username, string Role);

public record RefreshTokenRequest(string Token, string RefreshToken);
