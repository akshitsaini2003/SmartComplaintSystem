using SmartComplaint.Application.DTOs;

namespace SmartComplaint.Application.Interfaces;

public interface IAuthService
{
    Task<string> RegisterAsync(RegisterDto dto);
    Task<string> VerifyOtpAsync(VerifyOtpDto dto);
    Task<LoginResponseDto> LoginAsync(LoginDto dto);
    Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
    Task<string> LogoutAsync(string refreshToken);
    Task<string> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<string> ResetPasswordAsync(ResetPasswordDto dto);
}