using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartComplaint.Application.DTOs;
using SmartComplaint.Application.Interfaces;

namespace SmartComplaint.API.Controllers;

[ApiController]
[Route("api/assignments")]
[Authorize]
public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentService _service;

    public AssignmentsController(IAssignmentService service) => _service = service;

    // POST /api/assignments
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Assign([FromBody] CreateAssignmentDto dto)
    {
        try
        {
            var result = await _service.AssignAsync(dto);
            return Ok(result);
        }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    // GET /api/assignments/agent/{agentId}
    [HttpGet("agent/{agentId}")]
    [Authorize(Roles = "Admin,Agent")]
    public async Task<IActionResult> GetByAgent(int agentId)
    {
        try
        {
            var result = await _service.GetByAgentAsync(agentId);
            return Ok(result);
        }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    // PUT /api/assignments/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reassign(int id, [FromBody] ReassignDto dto)
    {
        try
        {
            var result = await _service.ReassignAsync(id, dto.NewAgentId);
            return Ok(result);
        }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }
}