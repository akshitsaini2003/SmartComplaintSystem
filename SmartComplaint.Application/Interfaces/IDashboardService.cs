using SmartComplaint.Application.DTOs;

namespace SmartComplaint.Application.Interfaces;

public interface IDashboardService
{
    Task<AdminDashboardDto> GetAdminDashboardAsync();
    Task<AgentDashboardDto> GetAgentDashboardAsync(int agentId);
    Task<UserDashboardDto> GetUserDashboardAsync(int userId);
    Task<ReportResultDto> GetReportAsync(ReportFilterDto filter);
}