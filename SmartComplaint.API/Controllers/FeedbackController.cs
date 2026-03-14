using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartComplaint.Application.DTOs;
using SmartComplaint.Application.Interfaces;

namespace SmartComplaint.API.Controllers;

[ApiController]
[Route("api/feedback")]
[Authorize]
public class FeedbackController : ControllerBase
{
    private readonly IFeedbackService _service;

    public FeedbackController(IFeedbackService service) => _service = service;

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Submit([FromBody] CreateFeedbackDto dto)
    {
        try { return Ok(await _service.SubmitAsync(dto, GetUserId())); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("{complaintId}")]
    public async Task<IActionResult> GetByComplaint(int complaintId)
    {
        try { return Ok(await _service.GetByComplaintAsync(complaintId)); }
        catch (Exception ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpGet("report")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetReport()
    {
        try { return Ok(await _service.GetReportAsync()); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }
}