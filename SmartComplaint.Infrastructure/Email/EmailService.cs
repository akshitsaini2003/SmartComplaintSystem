using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SmartComplaint.Application.Interfaces;

namespace SmartComplaint.Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        var settings = _config.GetSection("EmailSettings");

        var fromEmail = settings["FromEmail"] ?? throw new Exception("FromEmail not configured.");
        var fromName = settings["FromName"] ?? "Smart Complaint System";
        var host = settings["Host"] ?? throw new Exception("SMTP Host not configured.");
        var username = settings["Username"] ?? throw new Exception("SMTP Username not configured.");
        var password = settings["Password"] ?? throw new Exception("SMTP Password not configured.");
        var port = int.Parse(settings["Port"] ?? "587");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendOtpEmailAsync(string toEmail, string name, string otp)
    {
        var html = $@"
        <div style='font-family:Arial;max-width:500px;margin:auto;padding:30px;
                     border:1px solid #eee;border-radius:10px;'>
            <h2 style='color:#4F46E5;'>Email Verification</h2>
            <p>Hello <b>{name}</b>,</p>
            <p>Your OTP code is:</p>
            <div style='font-size:36px;font-weight:bold;color:#4F46E5;
                        letter-spacing:8px;text-align:center;padding:20px;
                        background:#F5F3FF;border-radius:8px;'>{otp}</div>
            <p style='color:#666;margin-top:20px;'>
                This OTP is valid for <b>10 minutes</b>. Do not share it with anyone.
            </p>
        </div>";

        await SendAsync(toEmail, name, "Verify Your Email — OTP", html);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string name, string resetToken)
    {
        var resetLink = $"https://yourapp.com/reset-password?token={resetToken}";
        var html = $@"
        <div style='font-family:Arial;max-width:500px;margin:auto;padding:30px;
                     border:1px solid #eee;border-radius:10px;'>
            <h2 style='color:#DC2626;'>Password Reset Request</h2>
            <p>Hello <b>{name}</b>,</p>
            <p>Click the button below to reset your password:</p>
            <a href='{resetLink}'
               style='display:inline-block;padding:12px 28px;background:#DC2626;
                      color:#fff;text-decoration:none;border-radius:6px;
                      font-weight:bold;margin:16px 0;'>Reset Password</a>
            <p style='color:#666;'>This link expires in <b>1 hour</b>.</p>
        </div>";

        await SendAsync(toEmail, name, "Reset Your Password", html);
    }

    public async Task SendComplaintCreatedEmailAsync(string toEmail, string name, string complaintTitle)
    {
        var html = $@"
        <div style='font-family:Arial;max-width:500px;margin:auto;padding:30px;
                     border:1px solid #eee;border-radius:10px;'>
            <h2 style='color:#059669;'>Complaint Submitted ✅</h2>
            <p>Hello <b>{name}</b>,</p>
            <p>Your complaint <b>'{complaintTitle}'</b> has been received.</p>
            <p style='color:#666;'>Our team will review it shortly.</p>
        </div>";

        await SendAsync(toEmail, name, "Complaint Received", html);
    }

    public async Task SendComplaintAssignedEmailAsync(string toEmail, string agentName, string complaintTitle)
    {
        var html = $@"
        <div style='font-family:Arial;max-width:500px;margin:auto;padding:30px;
                     border:1px solid #eee;border-radius:10px;'>
            <h2 style='color:#0284C7;'>New Complaint Assigned</h2>
            <p>Hello <b>{agentName}</b>,</p>
            <p>You have been assigned complaint: <b>'{complaintTitle}'</b></p>
            <p style='color:#666;'>Please log in to take action.</p>
        </div>";

        await SendAsync(toEmail, agentName, "Complaint Assigned to You", html);
    }

    public async Task SendStatusChangedEmailAsync(string toEmail, string name,
        string complaintTitle, string newStatus)
    {
        var html = $@"
        <div style='font-family:Arial;max-width:500px;margin:auto;padding:30px;
                     border:1px solid #eee;border-radius:10px;'>
            <h2 style='color:#7C3AED;'>Complaint Status Updated</h2>
            <p>Hello <b>{name}</b>,</p>
            <p>Your complaint <b>'{complaintTitle}'</b> status changed to:
               <b style='color:#7C3AED;'>{newStatus}</b></p>
        </div>";

        await SendAsync(toEmail, name, "Complaint Status Update", html);
    }

    public async Task SendSlaBreachEmailAsync(string toEmail, string complaintTitle, string priority)
    {
        var html = $@"
        <div style='font-family:Arial;max-width:500px;margin:auto;padding:30px;
                     border:1px solid #fee2e2;border-radius:10px;background:#fff5f5;'>
            <h2 style='color:#DC2626;'>⚠️ SLA Breach Alert</h2>
            <p>Complaint <b>'{complaintTitle}'</b> has breached SLA.</p>
            <p>Priority: <b style='color:#DC2626;'>{priority}</b></p>
            <p style='color:#666;'>Immediate action required.</p>
        </div>";

        await SendAsync(toEmail, "Admin", "SLA Breach Alert", html);
    }

    public async Task SendComplaintResolvedEmailAsync(string toEmail, string name, string complaintTitle)
    {
        var html = $@"
        <div style='font-family:Arial;max-width:500px;margin:auto;padding:30px;
                     border:1px solid #eee;border-radius:10px;'>
            <h2 style='color:#059669;'>Complaint Resolved 🎉</h2>
            <p>Hello <b>{name}</b>,</p>
            <p>Your complaint <b>'{complaintTitle}'</b> has been resolved.</p>
            <p style='color:#666;'>Please share your feedback to help us improve!</p>
        </div>";

        await SendAsync(toEmail, name, "Complaint Resolved", html);
    }
}