using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SmartComplaint.Application.DTOs;
using SmartComplaint.Application.Interfaces;
using SmartComplaint.Domain.Entities;

namespace SmartComplaint.Infrastructure.Services;

public class AttachmentService : IAttachmentService
{
    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _config;

    public AttachmentService(IUnitOfWork uow, IConfiguration config)
    {
        _uow = uow;
        _config = config;
    }

    public async Task<AttachmentResponseDto> UploadAsync(int complaintId, IFormFile file)
    {
        // ── Validate complaint exists ──────────────────────
        var complaint = await _uow.Complaints.GetByIdAsync(complaintId);
        if (complaint == null || complaint.IsDeleted)
            throw new Exception("Complaint not found.");

        // ── Validate file extension ────────────────────────
        var allowedExtensions = _config
            .GetSection("FileUpload:AllowedExtensions")
            .Get<string[]>() ?? Array.Empty<string>();

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(extension))
            throw new Exception($"File type '{extension}' is not allowed.");

        // ── Validate file size ─────────────────────────────
        var maxSizeMb = _config.GetValue<int>("FileUpload:MaxFileSizeMB");
        if (file.Length > maxSizeMb * 1024 * 1024)
            throw new Exception($"File size exceeds {maxSizeMb}MB limit.");

        // ── Save file to wwwroot/uploads ───────────────────
        var uploadPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            _config["FileUpload:UploadPath"] ?? "wwwroot/uploads");

        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        var fullPath = Path.Combine(uploadPath, uniqueFileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
            await file.CopyToAsync(stream);

        // ── Save to DB ─────────────────────────────────────
        var attachment = new Attachment
        {
            ComplaintId = complaintId,
            FileName = file.FileName,
            FilePath = $"/uploads/{uniqueFileName}",
            UploadedDate = DateTime.UtcNow,
        };

        await _uow.Attachments.AddAsync(attachment);
        await _uow.SaveChangesAsync();

        return new AttachmentResponseDto
        {
            AttachmentId = attachment.AttachmentId,
            FileName = attachment.FileName,
            FilePath = attachment.FilePath,
            UploadedDate = attachment.UploadedDate,
        };
    }

    public async Task<IEnumerable<AttachmentResponseDto>> GetByComplaintAsync(int complaintId)
    {
        var attachments = await _uow.Attachments
            .FindAsync(a => a.ComplaintId == complaintId);

        return attachments.Select(a => new AttachmentResponseDto
        {
            AttachmentId = a.AttachmentId,
            FileName = a.FileName,
            FilePath = a.FilePath,
            UploadedDate = a.UploadedDate,
        });
    }

    public async Task<string> DeleteAsync(int attachmentId)
    {
        var attachment = await _uow.Attachments.GetByIdAsync(attachmentId);
        if (attachment == null) throw new Exception("Attachment not found.");

        // ── Delete physical file ───────────────────────────
        var fullPath = Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot",
            attachment.FilePath.TrimStart('/'));

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        _uow.Attachments.Remove(attachment);
        await _uow.SaveChangesAsync();

        return "Attachment deleted successfully.";
    }
}