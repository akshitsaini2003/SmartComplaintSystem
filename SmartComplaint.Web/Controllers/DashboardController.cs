using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartComplaint.Web.Models;
using SmartComplaint.Web.Services;

namespace SmartComplaint.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApiService _api;
    public DashboardController(ApiService api) => _api = api;

    // ─── Admin Dashboard ─────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Admin()
    {
        var data = await _api.GetAsync<AdminDashboardModel>("api/dashboard/admin")
                   ?? new AdminDashboardModel();
        return View(data);
    }

    // ─── Agent Dashboard ─────────────────────────────────────
    [Authorize(Roles = "Admin,Agent")]
    public async Task<IActionResult> Agent()
    {
        var data = await _api.GetAsync<AgentDashboardModel>("api/dashboard/agent")
                   ?? new AgentDashboardModel();
        return View(data);
    }

    // ─── User Dashboard ──────────────────────────────────────
    [Authorize(Roles = "User")]
    public async Task<IActionResult> User()
    {
        var data = await _api.GetAsync<UserDashboardModel>("api/dashboard/user")
                   ?? new UserDashboardModel();
        return View(data);
    }

    // ─── Reports ─────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reports(ReportFilterModel? filter)
    {
        var query = "api/dashboard/reports?";
        if (!string.IsNullOrEmpty(filter?.FromDate)) query += $"fromDate={filter.FromDate}&";
        if (!string.IsNullOrEmpty(filter?.ToDate)) query += $"toDate={filter.ToDate}&";
        if (!string.IsNullOrEmpty(filter?.Category)) query += $"category={filter.Category}&";
        if (!string.IsNullOrEmpty(filter?.Priority)) query += $"priority={filter.Priority}&";
        if (!string.IsNullOrEmpty(filter?.Status)) query += $"status={filter.Status}&";
        if (filter?.AgentId.HasValue == true) query += $"agentId={filter.AgentId}&";

        var result = await _api.GetAsync<ReportResultModel>(query.TrimEnd('&', '?'))
                     ?? new ReportResultModel();

        var agents = await _api.GetAsync<List<UserModel>>("api/users/agents")
                     ?? new List<UserModel>();

        ViewBag.Filter = filter;
        ViewBag.Agents = agents;
        return View(result);
    }
}