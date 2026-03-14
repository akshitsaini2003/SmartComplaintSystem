using Microsoft.EntityFrameworkCore;
using SmartComplaint.Application.DTOs;
using SmartComplaint.Application.Interfaces;
using SmartComplaint.Domain.Entities;
using SmartComplaint.Domain.Enums;
using SmartComplaint.Infrastructure.Data;

namespace SmartComplaint.Infrastructure.Services;

public class FeedbackService : IFeedbackService
{
    private readonly IUnitOfWork _uow;
    private readonly AppDbContext _context;

    public FeedbackService(IUnitOfWork uow, AppDbContext context)
    {
        _uow = uow;
        _context = context;
    }

    public async Task<FeedbackResponseDto> SubmitAsync(CreateFeedbackDto dto, int userId)
    {
        var complaint = await _uow.Complaints.GetByIdAsync(dto.ComplaintId);
        if (complaint == null || complaint.IsDeleted)
            throw new Exception("Complaint not found.");

        if (complaint.Status != ComplaintStatus.Resolved &&
            complaint.Status != ComplaintStatus.Closed)
            throw new Exception("Feedback can only be submitted for Resolved/Closed complaints.");

        if (complaint.UserId != userId)
            throw new Exception("You can only submit feedback for your own complaints.");

        var existing = await _uow.Feedbacks
            .AnyAsync(f => f.ComplaintId == dto.ComplaintId && f.UserId == userId);
        if (existing) throw new Exception("Feedback already submitted.");

        if (dto.Rating < 1 || dto.Rating > 5)
            throw new Exception("Rating must be between 1 and 5.");

        var feedback = new Feedback
        {
            ComplaintId = dto.ComplaintId,
            UserId = userId,
            Rating = dto.Rating,
            Comment = dto.Comment,
        };

        await _uow.Feedbacks.AddAsync(feedback);
        await _uow.SaveChangesAsync();

        var user = await _uow.Users.GetByIdAsync(userId);

        return new FeedbackResponseDto
        {
            FeedbackId = feedback.FeedbackId,
            ComplaintId = feedback.ComplaintId,
            UserName = user?.Name ?? "Unknown",
            Rating = feedback.Rating,
            Comment = feedback.Comment,
            CreatedDate = feedback.CreatedDate,
        };
    }

    public async Task<FeedbackResponseDto> GetByComplaintAsync(int complaintId)
    {
        var feedback = await _context.Feedbacks
            .Include(f => f.User)
            .FirstOrDefaultAsync(f => f.ComplaintId == complaintId);

        if (feedback == null) throw new Exception("No feedback found.");

        return new FeedbackResponseDto
        {
            FeedbackId = feedback.FeedbackId,
            ComplaintId = feedback.ComplaintId,
            UserName = feedback.User.Name,
            Rating = feedback.Rating,
            Comment = feedback.Comment,
            CreatedDate = feedback.CreatedDate,
        };
    }

    public async Task<FeedbackReportDto> GetReportAsync()
    {
        var feedbacks = await _uow.Feedbacks.GetAllAsync();
        var list = feedbacks.ToList();

        return new FeedbackReportDto
        {
            AverageRating = list.Count > 0 ? Math.Round(list.Average(f => f.Rating), 2) : 0,
            TotalFeedbacks = list.Count,
        };
    }
}