using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartComplaint.Web.Models;
using SmartComplaint.Web.Services;

namespace SmartComplaint.Web.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly ApiService _api;
    public NotificationsController(ApiService api) => _api = api;

    // GET /Notifications
    [Route("Notifications")]
    [Route("Notifications/Index")]
    public async Task<IActionResult> Index()
    {
        var notifications = await _api.GetAsync<List<NotificationModel>>("api/notifications")
                            ?? new List<NotificationModel>();
        return View(notifications);
    }

    // AJAX — bell badge count
    public async Task<IActionResult> UnreadCount()
    {
        try
        {
            var notifications = await _api.GetAsync<List<NotificationModel>>("api/notifications")
                                ?? new List<NotificationModel>();
            var count = notifications.Count(n => !n.IsRead);
            return Json(new { count });
        }
        catch { return Json(new { count = 0 }); }
    }

    // PATCH — mark single as read
    public async Task<IActionResult> MarkRead(int id)
    {
        await _api.PatchAsync<object>($"api/notifications/{id}/read", new { });
        return RedirectToAction("Index");
    }

    // PATCH — mark all as read
    public async Task<IActionResult> MarkAllRead()
    {
        await _api.PatchAsync<object>("api/notifications/read-all", new { });
        return RedirectToAction("Index");
    }

    // DELETE
    public async Task<IActionResult> Delete(int id)
    {
        await _api.DeleteAsync<object>($"api/notifications/{id}");
        return RedirectToAction("Index");
    }
}