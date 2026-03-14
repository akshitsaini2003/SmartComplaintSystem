using SmartComplaint.Application.DTOs;
using SmartComplaint.Application.Interfaces;
using SmartComplaint.Domain.Entities;
using SmartComplaint.Domain.Enums;

namespace SmartComplaint.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;

    public UserService(IUnitOfWork uow) => _uow = uow;

    public async Task<UserResponseDto> CreateAsync(CreateUserDto dto)
    {
        var exists = await _uow.Users.AnyAsync(u => u.Email == dto.Email);
        if (exists) throw new Exception("Email already registered.");

        if (!Enum.TryParse<UserRole>(dto.Role, true, out var role))
            throw new Exception("Invalid role.");

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = role,
            IsActive = true,
            IsEmailVerified = true, // Admin-created users skip OTP
        };

        await _uow.Users.AddAsync(user);
        await _uow.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<PagedResult<UserResponseDto>> GetAllAsync(int page, int pageSize)
    {
        var all = await _uow.Users.FindAsync(u => !u.IsDeleted);
        var total = all.Count();
        var items = all
            .OrderByDescending(u => u.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .ToList();

        return new PagedResult<UserResponseDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<UserResponseDto> GetByIdAsync(int id)
    {
        var user = await _uow.Users.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
            throw new Exception("User not found.");
        return MapToDto(user);
    }

    public async Task<UserResponseDto> UpdateAsync(int id, UpdateUserDto dto)
    {
        var user = await _uow.Users.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
            throw new Exception("User not found.");

        user.Name = dto.Name;
        user.IsActive = dto.IsActive;

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<string> DeleteAsync(int id)
    {
        var user = await _uow.Users.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
            throw new Exception("User not found.");

        user.IsDeleted = true;
        user.IsActive = false;

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        return "User deactivated successfully.";
    }

    public async Task<IEnumerable<UserResponseDto>> GetAgentsAsync()
    {
        var agents = await _uow.Users
            .FindAsync(u => u.Role == UserRole.Agent && !u.IsDeleted && u.IsActive);
        return agents.Select(MapToDto);
    }

    public async Task<string> UpdateRoleAsync(int id, UpdateRoleDto dto)
    {
        var user = await _uow.Users.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
            throw new Exception("User not found.");

        if (!Enum.TryParse<UserRole>(dto.Role, true, out var role))
            throw new Exception("Invalid role.");

        user.Role = role;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        return $"Role updated to {role}.";
    }

    private UserResponseDto MapToDto(User u) => new()
    {
        UserId = u.UserId,
        Name = u.Name,
        Email = u.Email,
        Role = u.Role.ToString(),
        IsActive = u.IsActive,
        IsEmailVerified = u.IsEmailVerified,
        CreatedDate = u.CreatedDate,
    };
}