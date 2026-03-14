using Microsoft.EntityFrameworkCore;
using SmartComplaint.Domain.Entities;
using SmartComplaint.Domain.Enums;

namespace SmartComplaint.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // ─── Categories ──────────────────────────────────────
        if (!await context.Categories.AnyAsync())
        {
            context.Categories.AddRange(
                new Category { Name = "Network", Description = "Network related issues" },
                new Category { Name = "Billing", Description = "Billing and payment issues" },
                new Category { Name = "Hardware", Description = "Hardware related complaints" },
                new Category { Name = "Software", Description = "Software and application issues" },
                new Category { Name = "Security", Description = "Security and access issues" },
                new Category { Name = "General", Description = "General inquiries" }
            );
            await context.SaveChangesAsync();
        }

        // ─── SLA Policies ────────────────────────────────────
        if (!await context.SLAPolicies.AnyAsync())
        {
            context.SLAPolicies.AddRange(
                new SLAPolicy { Priority = Priority.High, MaxResolutionHours = 4 },
                new SLAPolicy { Priority = Priority.Medium, MaxResolutionHours = 12 },
                new SLAPolicy { Priority = Priority.Low, MaxResolutionHours = 24 }
            );
            await context.SaveChangesAsync();
        }

        // ─── Admin User ──────────────────────────────────────
        if (!await context.Users.AnyAsync())
        {
            context.Users.Add(new User
            {
                Name = "Super Admin",
                Email = "admin@smartcomplaint.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin,
                IsActive = true,
                IsEmailVerified = true,
            });
            await context.SaveChangesAsync();
        }
    }
}