using Microsoft.EntityFrameworkCore;
using SmartComplaint.Application.DTOs;
using SmartComplaint.Application.Interfaces;
using SmartComplaint.Domain.Entities;
using SmartComplaint.Infrastructure.Data;

namespace SmartComplaint.Infrastructure.Services;

public class CommentService : ICommentService
{
    private readonly IUnitOfWork _uow;
    private readonly AppDbContext _context;

    public CommentService(IUnitOfWork uow, AppDbContext context)
    {
        _uow = uow;
        _context = context;
    }

    public async Task<CommentResponseDto> AddAsync(CreateCommentDto dto, int userId)
    {
        var complaint = await _uow.Complaints.GetByIdAsync(dto.ComplaintId);
        if (complaint == null || complaint.IsDeleted)
            throw new Exception("Complaint not found.");

        var comment = new Comment
        {
            ComplaintId = dto.ComplaintId,
            UserId = userId,
            Message = dto.Message,
        };

        await _uow.Comments.AddAsync(comment);
        await _uow.SaveChangesAsync();

        var user = await _uow.Users.GetByIdAsync(userId);

        return new CommentResponseDto
        {
            CommentId = comment.CommentId,
            ComplaintId = comment.ComplaintId,
            Message = comment.Message,
            UserName = user?.Name ?? "Unknown",
            CreatedDate = comment.CreatedDate,
        };
    }

    public async Task<IEnumerable<CommentResponseDto>> GetByComplaintAsync(int complaintId)
    {
        var comments = await _context.Comments
            .Include(c => c.User)
            .Where(c => c.ComplaintId == complaintId && !c.IsDeleted)
            .OrderBy(c => c.CreatedDate)
            .ToListAsync();

        return comments.Select(c => new CommentResponseDto
        {
            CommentId = c.CommentId,
            ComplaintId = c.ComplaintId,
            Message = c.Message,
            UserName = c.User.Name,
            CreatedDate = c.CreatedDate,
        });
    }

    public async Task<string> DeleteAsync(int commentId, int userId, string role)
    {
        var comment = await _uow.Comments.GetByIdAsync(commentId);
        if (comment == null || comment.IsDeleted)
            throw new Exception("Comment not found.");

        // Only owner or Admin can delete
        if (comment.UserId != userId && role != "Admin")
            throw new Exception("Unauthorized to delete this comment.");

        comment.IsDeleted = true;
        _uow.Comments.Update(comment);
        await _uow.SaveChangesAsync();

        return "Comment deleted successfully.";
    }
}