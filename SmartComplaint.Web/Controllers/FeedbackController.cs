using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartComplaint.Web.Models;
using SmartComplaint.Web.Services;

namespace SmartComplaint.Web.Controllers;

[Authorize]
public class FeedbackController : Controller
{
    private readonly ApiService _api;
    public FeedbackController(ApiService api) => _api = api;

    // GET /Feedback/Submit/{complaintId}
    [Authorize(Roles = "User")]
    public IActionResult Submit(int id)
    {
        var model = new CreateFeedbackModel { ComplaintId = id };
        return View(model);
    }

    // POST /Feedback/Submit
    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Submit(CreateFeedbackModel model)
    {
        var result = await _api.PostAsync<FeedbackModel>("api/feedback", new
        {
            complaintId = model.ComplaintId,
            rating = model.Rating,
            comment = model.Comment
        });

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return View(model);
        }

        TempData["Success"] = "Thank you for your feedback!";
        return RedirectToAction("My", "Complaints");
    }

    // GET /Feedback/Report (Admin)
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Report()
    {
        var report = await _api.GetAsync<FeedbackReportModel>("api/feedback/report")
                     ?? new FeedbackReportModel();
        return View(report);
    }
}