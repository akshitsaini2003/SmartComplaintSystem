using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartComplaint.Application.DTOs;
using SmartComplaint.Application.Interfaces;

namespace SmartComplaint.API.Controllers;

[ApiController]
[Route("api/comments")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _service;

    public CommentsController(ICommentService service) => _service = service;

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private string GetRole() =>
        User.FindFirstValue(ClaimTypes.Role) ?? "User";

    // POST /api/comments
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateCommentDto dto)
    {
        try
        {
            var result = await _service.AddAsync(dto, GetUserId());
            return Ok(result);
        }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    // GET /api/comments/{complaintId}
    [HttpGet("{complaintId}")]
    public async Task<IActionResult> GetAll(int complaintId)
    {
        try
        {
            var result = await _service.GetByComplaintAsync(complaintId);
            return Ok(result);
        }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    // DELETE /api/comments/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _service.DeleteAsync(id, GetUserId(), GetRole());
            return Ok(new { message = result });
        }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }
}