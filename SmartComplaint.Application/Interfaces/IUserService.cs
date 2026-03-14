using SmartComplaint.Application.DTOs;

namespace SmartComplaint.Application.Interfaces;

public interface IUserService
{
    Task<UserResponseDto> CreateAsync(CreateUserDto dto);
    Task<PagedResult<UserResponseDto>> GetAllAsync(int page, int pageSize);
    Task<UserResponseDto> GetByIdAsync(int id);
    Task<UserResponseDto> UpdateAsync(int id, UpdateUserDto dto);
    Task<string> DeleteAsync(int id);
    Task<IEnumerable<UserResponseDto>> GetAgentsAsync();
    Task<string> UpdateRoleAsync(int id, UpdateRoleDto dto);
}