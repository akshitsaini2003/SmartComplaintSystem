using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartComplaint.Application.DTOs;
using SmartComplaint.Application.Interfaces;

namespace SmartComplaint.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service) => _service = service;

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET /api/dashboard/admin
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdminDashboard()
    {
        try
        {
            var result = await _service.GetAdminDashboardAsync();
            return Ok(result);
        }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    // GET /api/dashboard/agent
    [HttpGet("agent")]
    [Authorize(Roles = "Admin,Agent")]
    public async Task<IActionResult> GetAgentDashboard([FromQuery] int? agentId)
    {
        try
        {
            // Agent apna dashboard dekhe, Admin kisi bhi agent ka
            var id = agentId ?? GetUserId();
            var result = await _service.GetAgentDashboardAsync(id);
            return Ok(result);
        }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    // GET /api/dashboard/user
    [HttpGet("user")]
    public async Task<IActionResult> GetUserDashboard()
    {
        try
        {
            var result = await _service.GetUserDashboardAsync(GetUserId());
            return Ok(result);
        }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    // GET /api/dashboard/reports
    [HttpGet("reports")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetReport([FromQuery] ReportFilterDto filter)
    {
        try
        {
            var result = await _service.GetReportAsync(filter);
            return Ok(result);
        }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }
}