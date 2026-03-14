using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartComplaint.Application.DTOs;
using SmartComplaint.Application.Interfaces;
using SmartComplaint.Domain.Entities;
using SmartComplaint.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace SmartComplaint.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUnitOfWork uow, IEmailService emailService, IConfiguration config,ILogger<AuthService> logger)
    {
        _uow = uow;
        _emailService = emailService;
        _config = config;
        _logger = logger;
    }

    // ─── Register ────────────────────────────────────────────
    public async Task<string> RegisterAsync(RegisterDto dto)
    {
        var exists = await _uow.Users.AnyAsync(u => u.Email == dto.Email);
        if (exists)
        {
            _logger.LogWarning("Registration failed — email already exists: {Email}", dto.Email);
            throw new Exception("Email already registered.");
        }

        var otp = GenerateOtp();

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.User,
            OTP = otp,
            OTPExpiry = DateTime.UtcNow.AddMinutes(10),
        };

        await _uow.Users.AddAsync(user);
        await _uow.SaveChangesAsync();

        await _emailService.SendOtpEmailAsync(dto.Email, dto.Name, otp);

        return "Registration successful. Check your email for OTP.";
    }

    // ─── Verify OTP ──────────────────────────────────────────
    public async Task<string> VerifyOtpAsync(VerifyOtpDto dto)
    {
        var user = await _uow.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null) throw new Exception("User not found.");
        if (user.OTP != dto.OTP) throw new Exception("Invalid OTP.");
        if (user.OTPExpiry < DateTime.UtcNow) throw new Exception("OTP expired.");

        user.IsEmailVerified = true;
        user.OTP = null;
        user.OTPExpiry = null;

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        return "Email verified successfully. You can now login.";
    }

    // ─── Login ───────────────────────────────────────────────
    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _uow.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", dto.Email);
            throw new Exception("Invalid credentials.");
        }
        if (!user.IsEmailVerified)
        {
            _logger.LogWarning("Unverified login attempt: {Email}", dto.Email);
            throw new Exception("Please verify your email first.");
        }
        if (!user.IsActive)
            throw new Exception("Account is deactivated.");

        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(
            _config.GetValue<int>("JwtSettings:RefreshTokenExpiryDays"));

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("User logged in: {Email} | Role: {Role}",
            user.Email, user.Role);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString(),
        };
    }

    // ─── Refresh Token ───────────────────────────────────────
    public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var user = await _uow.Users.FirstOrDefaultAsync(u =>
            u.RefreshToken == refreshToken && u.RefreshTokenExpiry > DateTime.UtcNow);

        if (user == null) throw new Exception("Invalid or expired refresh token.");

        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(
            _config.GetValue<int>("JwtSettings:RefreshTokenExpiryDays"));

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        return new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString(),
        };
    }

    // ─── Logout ──────────────────────────────────────────────
    public async Task<string> LogoutAsync(string refreshToken)
    {
        var user = await _uow.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        if (user == null) throw new Exception("Invalid token.");

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        return "Logged out successfully.";
    }

    // ─── Forgot Password ─────────────────────────────────────
    public async Task<string> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _uow.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null) throw new Exception("Email not found.");

        var token = Guid.NewGuid().ToString();
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        await _emailService.SendPasswordResetEmailAsync(dto.Email, user.Name, token);

        return "Password reset link sent to your email.";
    }

    // ─── Reset Password ──────────────────────────────────────
    public async Task<string> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _uow.Users.FirstOrDefaultAsync(u =>
            u.PasswordResetToken == dto.Token &&
            u.PasswordResetTokenExpiry > DateTime.UtcNow);

        if (user == null) throw new Exception("Invalid or expired reset token.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        return "Password reset successfully.";
    }

    // ─── Helpers ─────────────────────────────────────────────
    private string GenerateOtp()
        => new Random().Next(100000, 999999).ToString();

    private string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private string GenerateAccessToken(User user)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email,          user.Email),
            new Claim(ClaimTypes.Name,           user.Name),
            new Claim(ClaimTypes.Role,           user.Role.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                                    _config.GetValue<int>("JwtSettings:AccessTokenExpiryMinutes")),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}