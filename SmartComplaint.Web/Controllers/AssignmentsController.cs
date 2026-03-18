using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartComplaint.Web.Models;
using SmartComplaint.Web.Services;
using System.Security.Claims;

namespace SmartComplaint.Web.Controllers;

[Authorize]
public class AssignmentsController : Controller
{
    private readonly ApiService _api;
    public AssignmentsController(ApiService api) => _api = api;

    // GET /Assignments (Admin)
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        var complaints = await _api.GetAsync<PagedResult<ComplaintListModel>>(
            "api/complaints?page=1&pageSize=100")
            ?? new PagedResult<ComplaintListModel>();

        var agents = await _api.GetAsync<List<UserModel>>("api/users/agents")
                     ?? new List<UserModel>();

        ViewBag.Complaints = complaints.Items
            .Where(c => c.Status == "Open" || c.Status == "InProgress")
            .ToList();
        ViewBag.Agents = agents;

        return View();
    }

    // GET /Assignments/MyAssignments (Agent)


    [Authorize(Roles = "Agent,Admin")]
    public async Task<IActionResult> MyAssignments()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdStr, out int agentId) || agentId == 0)
        {
            TempData["Error"] = "Session issue — please logout and login again.";
            return View(new List<AssignmentModel>());
        }

        var assignments = await _api.GetAsync<List<AssignmentModel>>(
            $"api/assignments/agent/{agentId}")
            ?? new List<AssignmentModel>();

        return View(assignments);
    }

    // POST /Assignments/Assign
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Assign(int complaintId, int agentId)
    {
        var result = await _api.PostAsync<AssignmentModel>("api/assignments", new
        {
            complaintId,
            agentId
        });

        if (!result.IsSuccess)
            TempData["Error"] = result.ErrorMessage;
        else
            TempData["Success"] = "Complaint assigned successfully!";

        return RedirectToAction("Index");
    }

    // POST /Assignments/Reassign
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reassign(int assignmentId, int newAgentId)
    {
        var result = await _api.PutAsync<AssignmentModel>(
            $"api/assignments/{assignmentId}", new { newAgentId });

        if (!result.IsSuccess)
            TempData["Error"] = result.ErrorMessage;
        else
            TempData["Success"] = "Complaint reassigned!";

        return RedirectToAction("Index");
    }
}