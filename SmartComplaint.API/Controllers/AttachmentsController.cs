using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartComplaint.Application.Interfaces;

namespace SmartComplaint.API.Controllers;

[ApiController]
[Route("api/complaints/{complaintId}/attach")]
[Authorize]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentService _service;

    public AttachmentsController(IAttachmentService service)
    {
        _service = service;
    }

    // POST /api/complaints/{complaintId}/attach
    [HttpPost]
    public async Task<IActionResult> Upload(int complaintId, IFormFile file)
    {
        try
        {
            var result = await _service.UploadAsync(complaintId, file);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /api/complaints/{complaintId}/attach
    [HttpGet]
    public async Task<IActionResult> GetAll(int complaintId)
    {
        try
        {
            var result = await _service.GetByComplaintAsync(complaintId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // DELETE /api/complaints/{complaintId}/attach/{attachmentId}
    [HttpDelete("{attachmentId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int complaintId, int attachmentId)
    {
        try
        {
            var result = await _service.DeleteAsync(attachmentId);
            return Ok(new { message = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}