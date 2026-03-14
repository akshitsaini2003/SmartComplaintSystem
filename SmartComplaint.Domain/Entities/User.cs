using SmartComplaint.Domain.Enums;

namespace SmartComplaint.Domain.Entities;

public class User : BaseEntity
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; } = false;
    public string? OTP { get; set; }
    public DateTime? OTPExpiry { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }

    // Navigation
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}