using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartComplaint.Web.Models;
using SmartComplaint.Web.Services;
using System.Security.Claims;

namespace SmartComplaint.Web.Controllers;

[Authorize]
public class ComplaintsController : Controller
{
    private readonly ApiService _api;
    public ComplaintsController(ApiService api) => _api = api;

    // ─── GET All (Admin/Agent) ────────────────────────────────
    [Authorize(Roles = "Admin,Agent")]
    public async Task<IActionResult> Index(
    int page = 1, string? status = null, string? priority = null)
    {
        if (User.IsInRole("Agent"))
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdStr, out int agentId) || agentId == 0)
            {
                TempData["Error"] = "Session issue — please logout and login again.";
                return View(new List<ComplaintListModel>());
            }

            var assignments = await _api.GetAsync<List<AssignmentModel>>(
                $"api/assignments/agent/{agentId}")
                ?? new List<AssignmentModel>();

            var complaintTasks = assignments.Select(a =>
                _api.GetAsync<ComplaintDetailModel>($"api/complaints/{a.ComplaintId}"));
            var details = await Task.WhenAll(complaintTasks);

            var agentComplaints = details
                .Where(d => d != null)
                .Select(d => new ComplaintListModel
                {
                    ComplaintId = d!.ComplaintId,
                    Title = d.Title,
                    Priority = d.Priority,
                    Status = d.Status,
                    CategoryName = d.CategoryName,
                    CreatedDate = d.CreatedDate,
                }).ToList();

            if (!string.IsNullOrEmpty(status))
                agentComplaints = agentComplaints
                    .Where(c => c.Status == status).ToList();

            if (!string.IsNullOrEmpty(priority))
                agentComplaints = agentComplaints
                    .Where(c => c.Priority == priority).ToList();

            ViewBag.CurrentPage = 1;
            ViewBag.TotalPages = 1;
            ViewBag.Status = status;
            ViewBag.Priority = priority;
            ViewBag.IsAgent = true;

            return View(agentComplaints);
        }

        // ── Admin ────────────────────────────────────────────────
        var query = $"api/complaints?page={page}&pageSize=10";
        if (!string.IsNullOrEmpty(status)) query += $"&status={status}";
        if (!string.IsNullOrEmpty(priority)) query += $"&priority={priority}";

        var result = await _api.GetAsync<PagedResult<ComplaintListModel>>(query)
                     ?? new PagedResult<ComplaintListModel>();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = result.TotalPages;
        ViewBag.Status = status;
        ViewBag.Priority = priority;
        ViewBag.IsAgent = false;

        return View(result.Items);
    }

    // ─── GET My Complaints (User) ─────────────────────────────
    [Authorize(Roles = "User")]
    public async Task<IActionResult> My(int page = 1)
    {
        var result = await _api.GetAsync<PagedResult<ComplaintListModel>>(
            $"api/complaints/my?page={page}&pageSize=10")
            ?? new PagedResult<ComplaintListModel>();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = result.TotalPages;

        return View(result.Items);
    }

    // ─── GET Detail ───────────────────────────────────────────    
    public async Task<IActionResult> Detail(int id)
    {
        var complaint = await _api.GetAsync<ComplaintDetailModel>(
            $"api/complaints/{id}");

        if (complaint == null) return NotFound();

        var comments = await _api.GetAsync<List<CommentModel>>(
            $"api/comments/{id}") ?? new List<CommentModel>();

        var attachments = await _api.GetAsync<List<AttachmentModel>>(
            $"api/complaints/{id}/attach") ?? new List<AttachmentModel>();

        // ✅ History fetch karo
        var history = await _api.GetAsync<List<ComplaintHistoryModel>>(
            $"api/complaints/{id}/history") ?? new List<ComplaintHistoryModel>();

        ViewBag.Comments = comments;
        ViewBag.Attachments = attachments;
        ViewBag.History = history;  // ✅ Add

        return View(complaint);
    }

    // ─── GET Create ───────────────────────────────────────────
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Create()
    {
        var categories = await _api.GetAsync<List<CategoryModel>>(
            "api/complaints") ?? new List<CategoryModel>();

        // Get categories from a workaround — fetch one complaint to get category list
        // Actually load from users endpoint or seed
        ViewBag.Categories = await GetCategoriesAsync();
        return View(new CreateComplaintModel());
    }

    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Create(CreateComplaintModel model)
    {
        var result = await _api.PostAsync<ComplaintDetailModel>("api/complaints", new
        {
            title = model.Title,
            description = model.Description,
            categoryId = model.CategoryId,
            priority = model.Priority
        });

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            ViewBag.Categories = await GetCategoriesAsync();
            return View(model);
        }

        TempData["Success"] = "Complaint submitted successfully!";
        return RedirectToAction("My");
    }

    // ─── GET Edit ─────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var complaint = await _api.GetAsync<ComplaintDetailModel>(
            $"api/complaints/{id}");

        if (complaint == null) return NotFound();

        ViewBag.Categories = await GetCategoriesAsync();

        var model = new UpdateComplaintModel
        {
            Title = complaint.Title,
            Description = complaint.Description,
        };
        ViewBag.ComplaintId = id;
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, UpdateComplaintModel model)
    {
        var result = await _api.PutAsync<ComplaintDetailModel>(
            $"api/complaints/{id}", new
            {
                title = model.Title,
                description = model.Description,
                categoryId = model.CategoryId
            });

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            ViewBag.Categories = await GetCategoriesAsync();
            ViewBag.ComplaintId = id;
            return View(model);
        }

        TempData["Success"] = "Complaint updated!";
        return RedirectToAction("Detail", new { id });
    }

    // ─── POST Update Status ───────────────────────────────────
    [HttpPost]
    [Authorize(Roles = "Admin,Agent")]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var result = await _api.PatchAsync<object>(
            $"api/complaints/{id}/status", new { status });

        if (!result.IsSuccess)
            TempData["Error"] = result.ErrorMessage;
        else
            TempData["Success"] = $"Status updated to {status}!";

        return RedirectToAction("Detail", new { id });
    }

    // ─── POST Delete ──────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _api.DeleteAsync<object>($"api/complaints/{id}");
        TempData["Success"] = "Complaint deleted!";
        return RedirectToAction("Index");
    }

    // ─── POST Add Comment ─────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> AddComment(int complaintId, string message)
    {
        var result = await _api.PostAsync<object>("api/comments", new
        {
            complaintId,
            message
        });

        if (!result.IsSuccess)
            TempData["Error"] = result.ErrorMessage;

        return RedirectToAction("Detail", new { id = complaintId });
    }

    // ─── POST Delete Comment ──────────────────────────────────
    public async Task<IActionResult> DeleteComment(int commentId, int complaintId)
    {
        await _api.DeleteAsync<object>($"api/comments/{commentId}");
        return RedirectToAction("Detail", new { id = complaintId });
    }

    // ─── POST Upload Attachment ───────────────────────────────
    [HttpPost]
    public async Task<IActionResult> UploadAttachment(int complaintId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please select a file.";
            return RedirectToAction("Detail", new { id = complaintId });
        }

        var result = await _api.UploadAsync<AttachmentModel>(
            $"api/complaints/{complaintId}/attach", file);

        if (!result.IsSuccess)
            TempData["Error"] = result.ErrorMessage;
        else
            TempData["Success"] = "File uploaded successfully!";

        return RedirectToAction("Detail", new { id = complaintId });
    }

    // ─── DELETE Attachment ────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAttachment(int attachmentId, int complaintId)
    {
        await _api.DeleteAsync<object>(
            $"api/complaints/{complaintId}/attach/{attachmentId}");
        return RedirectToAction("Detail", new { id = complaintId });
    }

    // ─── Helper — Load Categories ─────────────────────────────
    private async Task<List<CategoryModel>> GetCategoriesAsync()
    {
        // Categories seedhi API nahi hai — DbContext se directly lenge
        // Workaround: hardcode seeded categories
        return new List<CategoryModel>
        {
            new() { CategoryId = 1, Name = "Network"  },
            new() { CategoryId = 2, Name = "Billing"  },
            new() { CategoryId = 3, Name = "Hardware" },
            new() { CategoryId = 4, Name = "Software" },
            new() { CategoryId = 5, Name = "Security" },
            new() { CategoryId = 6, Name = "General"  },
        };
    }


   
}