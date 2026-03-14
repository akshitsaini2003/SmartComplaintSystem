using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartComplaint.Application.DTOs;
using SmartComplaint.Application.Interfaces;

namespace SmartComplaint.API.Controllers;

[ApiController]
[Route("api/complaints")]
[Authorize]
public class ComplaintsController : ControllerBase
{
    private readonly IComplaintService _service;

    public ComplaintsController(IComplaintService service)
    {
        _service = service;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private string GetUserName() =>
        User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

    // POST /api/complaints
    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Create([FromBody] CreateComplaintDto dto)
    {
        try
        {
            var result = await _service.CreateAsync(dto, GetUserId());
            return Ok(result);
        }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    // GET /api/complaints
    [HttpGet]
    [Authorize(Roles = "Admin,Agent")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null)
    {
        try
        {
            var result = await _service.GetAllAsync(page, pageSize, status, priority);
            return Ok(result);
        }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    // GET /api/complaints/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(result);
        }
        catch (Exception ex) { return NotFound(new { message = ex.Message }); }
    }

    // PUT /api/complaints/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateComplaintDto dto)
    {
        try
        {
            var result = await _service.UpdateAsync(id, dto, GetUserId());
            return Ok(result);
        }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    // DELETE /api/complaints/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            return Ok(new { message = result });
        }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    // GET /api/complaints/my
    [HttpGet("my")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> GetMy(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _service.GetMyComplaintsAsync(GetUserId(), page, pageSize);
            return Ok(result);
        }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    // PATCH /api/complaints/{id}/status
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Agent")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
    {
        try
        {
            var result = await _service.UpdateStatusAsync(id, dto, GetUserName());
            return Ok(new { message = result });
        }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }
}