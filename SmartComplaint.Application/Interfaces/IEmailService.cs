namespace SmartComplaint.Application.Interfaces;

public interface IEmailService
{
    Task SendOtpEmailAsync(string toEmail, string name, string otp);
    Task SendPasswordResetEmailAsync(string toEmail, string name, string resetToken);
    Task SendComplaintCreatedEmailAsync(string toEmail, string name, string complaintTitle);
    Task SendComplaintAssignedEmailAsync(string toEmail, string agentName, string complaintTitle);
    Task SendStatusChangedEmailAsync(string toEmail, string name, string complaintTitle, string newStatus);
    Task SendSlaBreachEmailAsync(string toEmail, string complaintTitle, string priority);
    Task SendComplaintResolvedEmailAsync(string toEmail, string name, string complaintTitle);
}