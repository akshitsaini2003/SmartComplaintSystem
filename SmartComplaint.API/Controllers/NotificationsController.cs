using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartComplaint.Application.Interfaces;

namespace SmartComplaint.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationsController(INotificationService service) => _service = service;

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetMy()
    {
        try { return Ok(await _service.GetMyAsync(GetUserId())); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        try { return Ok(new { message = await _service.MarkReadAsync(id, GetUserId()) }); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        try { return Ok(new { message = await _service.MarkAllReadAsync(GetUserId()) }); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { return Ok(new { message = await _service.DeleteAsync(id, GetUserId()) }); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }
}