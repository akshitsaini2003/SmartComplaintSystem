using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SmartComplaint.Application.DTOs;
using SmartComplaint.Application.Interfaces;
using SmartComplaint.Domain.Entities;
using SmartComplaint.Domain.Enums;
using SmartComplaint.Infrastructure.Services;
using System.Text;
using Xunit;

namespace SmartComplaint.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IEmailService> _emailMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _emailMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        // Mock JWT config
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "JwtSettings:SecretKey",                "TestSuperSecretKey@2024#Minimum32Chars!" },
            { "JwtSettings:Issuer",                   "SmartComplaintSystem" },
            { "JwtSettings:Audience",                 "SmartComplaintUsers" },
            { "JwtSettings:AccessTokenExpiryMinutes", "15" },
            { "JwtSettings:RefreshTokenExpiryDays",   "7"  },
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _authService = new AuthService(
            _uowMock.Object,
            _emailMock.Object,
            configuration,       // real config, not mock
            _loggerMock.Object);


    
    }

    // ─── Register Tests ───────────────────────────────────────

    [Fact]
    public async Task Register_ShouldThrow_WhenEmailAlreadyExists()
    {
        // Arrange
        _uowMock.Setup(u => u.Users.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(true);

        var dto = new RegisterDto
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "Test@123"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _authService.RegisterAsync(dto));

        Assert.Equal("Email already registered.", ex.Message);
    }

    [Fact]
    public async Task Register_ShouldSucceed_WhenEmailIsNew()
    {
        // Arrange
        _uowMock.Setup(u => u.Users.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(false);
        _uowMock.Setup(u => u.Users.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);
        _emailMock.Setup(e => e.SendOtpEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var dto = new RegisterDto
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "Test@123"
        };

        // Act
        var result = await _authService.RegisterAsync(dto);

        // Assert
        Assert.Contains("Registration successful", result);
        _emailMock.Verify(e => e.SendOtpEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    // ─── OTP Tests ────────────────────────────────────────────

    [Fact]
    public async Task VerifyOtp_ShouldThrow_WhenOtpIsInvalid()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            OTP = "123456",
            OTPExpiry = DateTime.UtcNow.AddMinutes(5),
        };

        _uowMock.Setup(u => u.Users.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(user);

        var dto = new VerifyOtpDto
        {
            Email = "test@example.com",
            OTP = "999999" // wrong OTP
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _authService.VerifyOtpAsync(dto));

        Assert.Equal("Invalid OTP.", ex.Message);
    }

    [Fact]
    public async Task VerifyOtp_ShouldThrow_WhenOtpIsExpired()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            OTP = "123456",
            OTPExpiry = DateTime.UtcNow.AddMinutes(-5), // expired
        };

        _uowMock.Setup(u => u.Users.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(user);

        var dto = new VerifyOtpDto
        {
            Email = "test@example.com",
            OTP = "123456"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _authService.VerifyOtpAsync(dto));

        Assert.Equal("OTP expired.", ex.Message);
    }

    // ─── Login Tests ──────────────────────────────────────────

    [Fact]
    public async Task Login_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        _uowMock.Setup(u => u.Users.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync((User?)null);

        var dto = new LoginDto
        {
            Email = "notfound@example.com",
            Password = "Test@123"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _authService.LoginAsync(dto));

        Assert.Equal("Invalid credentials.", ex.Message);
    }

    [Fact]
    public async Task Login_ShouldThrow_WhenEmailNotVerified()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            IsEmailVerified = false,
            IsActive = true,
            Role = UserRole.User,
        };

        _uowMock.Setup(u => u.Users.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(user);

        var dto = new LoginDto
        {
            Email = "test@example.com",
            Password = "Test@123"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _authService.LoginAsync(dto));

        Assert.Equal("Please verify your email first.", ex.Message);
    }
}
