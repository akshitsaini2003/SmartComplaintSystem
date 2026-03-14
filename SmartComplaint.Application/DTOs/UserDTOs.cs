namespace SmartComplaint.Application.DTOs;

public class UserResponseDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
}

public class UpdateUserDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class UpdateRoleDto
{
    public string Role { get; set; } = string.Empty;
}