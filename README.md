# 🎯 Smart Complaint Management System

> **Enterprise-level Complaint Management API** built with ASP.NET Core 8, Clean Architecture, SQL Server, JWT Authentication, and Background Jobs.

---

## 📋 Table of Contents

- [Project Overview](#project-overview)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Database Schema](#database-schema)
- [Features](#features)
- [API Endpoints](#api-endpoints)
- [Prerequisites](#prerequisites)
- [Setup & Installation](#setup--installation)
- [Configuration](#configuration)
- [Running the Project](#running-the-project)
- [Default Admin Credentials](#default-admin-credentials)
- [Background Jobs](#background-jobs)
- [SLA Policy](#sla-policy)
- [Email Events](#email-events)
- [Running Tests](#running-tests)
- [Project Structure](#project-structure)

---

## 📌 Project Overview

The **Smart Complaint Management System** is a production-ready REST API that allows organizations to manage customer complaints end-to-end. It supports three user roles — **User**, **Agent**, and **Admin** — each with their own set of permissions and dashboards.

Key capabilities include:
- Email OTP-based registration & JWT authentication
- Smart auto-priority detection using keyword analysis
- Complaint lifecycle management with strict status transitions
- SLA breach monitoring via Hangfire background jobs
- File attachments, comments, feedback, and in-app notifications
- Role-based analytics dashboards and report filtering
- Structured logging via Serilog to Console, File, and SQL Server

---

## 🛠️ Tech Stack

| Technology | Purpose |
|---|---|
| **ASP.NET Core 8** | Web API framework |
| **SQL Server (SSMS)** | Primary database |
| **Entity Framework Core 8** | ORM — Code First migrations |
| **JWT Bearer** | Authentication & authorization |
| **BCrypt.Net** | Password hashing |
| **MailKit** | SMTP email sending |
| **AutoMapper** | Entity ↔ DTO mapping |
| **FluentValidation** | Request validation |
| **Hangfire** | Background job scheduling |
| **Serilog** | Structured logging |
| **Swagger / Swashbuckle** | API documentation |
| **AspNetCoreRateLimit** | API rate limiting |
| **xUnit + Moq** | Unit testing |

---

## 🏗️ Architecture

The project follows **Clean Architecture** with 5 projects in one solution:

```
SmartComplaintSystem/
├── SmartComplaint.API              → Controllers, Middleware, Program.cs
├── SmartComplaint.Application      → Services Interfaces, DTOs, Validators, Mappings
├── SmartComplaint.Domain           → Entities, Enums, Constants
├── SmartComplaint.Infrastructure   → DbContext, Repositories, Email, Background Jobs
└── SmartComplaint.Tests            → Unit Tests (xUnit + Moq)
```

**Dependency Flow:**
```
API → Application → Domain
API → Infrastructure → Application → Domain
```

**Patterns Used:**
- Repository Pattern with Generic Repository `IRepository<T>`
- Unit of Work Pattern
- Service Layer (business logic separated from controllers)
- DTOs for all request/response (entities never exposed directly)
- Soft Delete (`IsDeleted` flag on all entities)
- Pagination on all list endpoints

---

## 🗄️ Database Schema

The system uses **11 database tables**:

| Table | Description |
|---|---|
| `Users` | All users (User / Agent / Admin roles) |
| `Categories` | Complaint categories (Network, Billing, etc.) |
| `Complaints` | Core complaint records |
| `ComplaintAssignments` | Agent assignments per complaint |
| `Comments` | Comments on complaints |
| `Attachments` | File attachments per complaint |
| `Notifications` | In-app notifications per user |
| `ComplaintHistory` | Status change audit trail |
| `SLAPolicies` | SLA hours per priority level |
| `Feedbacks` | User ratings after resolution |
| `AuditLogs` | System-wide audit log |

---

## ✨ Features

### 🔐 Authentication & Security
- Email OTP registration with 10-minute expiry
- JWT Access Token (15 min) + Refresh Token (7 days)
- Forgot/Reset password via secure email link (1-hour expiry)
- BCrypt password hashing
- Role-based authorization: `User`, `Agent`, `Admin`
- API Rate Limiting (60 requests/minute)
- CORS policy
- Global exception handling middleware

### 📝 Complaint Management
- Full CRUD with pagination and filters
- **Smart Priority Auto-Detection** via keyword scan:
  - `High` → "server down", "emergency", "critical", "outage"
  - `Medium` → "slow", "intermittent", "degraded", "delay"
  - `Low` → default fallback
- **Status Transition Engine** (strict flow):
  ```
  Open → InProgress → OnHold → Resolved → Closed
  ```
- Complete status change history logged in `ComplaintHistory`
- File attachment upload (jpg, jpeg, png, pdf, docx — max 5MB)
- Soft delete on all records

### 👥 User Management
- Full CRUD for users (Admin only)
- Role promotion/demotion
- Agent listing for assignment

### 📋 Assignment Module
- Manual assignment of complaints to agents
- Reassignment support
- In-app + email notification to agent on assignment

### 💬 Comments
- Users and agents can comment on complaints
- Comment owner or Admin can delete

### 🔔 Notifications
- In-app notifications triggered on every major event
- Mark single/all as read
- Delete notifications

### ⭐ Feedback
- Users can rate resolved complaints (1–5 stars)
- Admin can view aggregate feedback report

### 📊 Analytics Dashboard
- **Admin:** Total complaints, open/resolved counts, SLA breaches, top agents, category & priority breakdown
- **Agent:** Assigned complaints, pending vs resolved, average resolution time
- **User:** Personal complaint stats and feedback average
- **Reports API:** Filter by date range, category, priority, status, agent

### ⏱️ SLA Monitoring
- Hangfire background job runs every 15 minutes
- Detects complaints exceeding SLA limits
- On breach: sends admin email, creates in-app notification, writes to AuditLogs

### 📦 Logging
- Serilog configured with 3 sinks: Console, File (daily rolling), SQL Server
- Request logging middleware logs every HTTP request
- Structured event logging for auth, complaints, emails, SLA, exceptions

---

## 📡 API Endpoints

### Auth
```
POST   /api/auth/register
POST   /api/auth/verify-otp
POST   /api/auth/login
POST   /api/auth/refresh-token
POST   /api/auth/logout
POST   /api/auth/forgot-password
POST   /api/auth/reset-password
```

### Complaints
```
POST   /api/complaints
GET    /api/complaints
GET    /api/complaints/{id}
PUT    /api/complaints/{id}
DELETE /api/complaints/{id}
GET    /api/complaints/my
PATCH  /api/complaints/{id}/status
```

### Attachments
```
POST   /api/complaints/{complaintId}/attach
GET    /api/complaints/{complaintId}/attach
DELETE /api/complaints/{complaintId}/attach/{attachmentId}
```

### Users
```
POST   /api/users
GET    /api/users
GET    /api/users/{id}
PUT    /api/users/{id}
DELETE /api/users/{id}
GET    /api/users/agents
PATCH  /api/users/{id}/role
```

### Assignments
```
POST   /api/assignments
GET    /api/assignments/agent/{agentId}
PUT    /api/assignments/{id}
```

### Comments
```
POST   /api/comments
GET    /api/comments/{complaintId}
DELETE /api/comments/{id}
```

### Notifications
```
GET    /api/notifications
PATCH  /api/notifications/{id}/read
PATCH  /api/notifications/read-all
DELETE /api/notifications/{id}
```

### Feedback
```
POST   /api/feedback
GET    /api/feedback/{complaintId}
GET    /api/feedback/report
```

### Dashboard
```
GET    /api/dashboard/admin
GET    /api/dashboard/agent
GET    /api/dashboard/user
GET    /api/dashboard/reports
```

---

## ✅ Prerequisites

Make sure the following are installed before running the project:

- [Visual Studio 2022](https://visualstudio.microsoft.com/) (v17.8+)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server) + [SSMS](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
- A Gmail account with **App Password** enabled (for email sending)

---

## 🚀 Setup & Installation

### Step 1: Clone the Repository

```bash
git clone https://github.com/yourusername/SmartComplaintSystem.git
cd SmartComplaintSystem
```

### Step 2: Configure appsettings.json

Open `SmartComplaint.API/appsettings.json` and update the following:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=SmartComplaintDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-gmail-app-password",
    "FromName": "Smart Complaint System",
    "FromEmail": "your-email@gmail.com"
  }
}
```

> ⚠️ **Gmail App Password:** Go to Google Account → Security → 2-Step Verification → App Passwords → Generate one for "Mail"

### Step 3: Apply Migrations

Open **Package Manager Console** in Visual Studio:

```powershell
# Set default project to Infrastructure
Add-Migration InitialCreate -StartupProject SmartComplaint.API
Update-Database -StartupProject SmartComplaint.API
```

This will:
- Create the `SmartComplaintDB` database
- Apply all 11 tables
- Automatically seed Categories, SLA Policies, and default Admin user

### Step 4: Build the Solution

```
Ctrl + Shift + B
```

Ensure **0 errors** before running.

---

## ⚙️ Configuration

Full `appsettings.json` reference:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=SmartComplaintDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "SmartComplaintSuperSecretKey@2024#Minimum32Chars!",
    "Issuer": "SmartComplaintSystem",
    "Audience": "SmartComplaintUsers",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "",
    "Password": "",
    "FromName": "Smart Complaint System",
    "FromEmail": ""
  },
  "SLASettings": {
    "HighPriorityHours": 4,
    "MediumPriorityHours": 12,
    "LowPriorityHours": 24
  },
  "FileUpload": {
    "AllowedExtensions": [ ".jpg", ".jpeg", ".png", ".pdf", ".docx" ],
    "MaxFileSizeMB": 5,
    "UploadPath": "wwwroot/uploads"
  },
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": false,
    "StackBlockedRequests": false,
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 60
      }
    ]
  }
}
```

---

## ▶️ Running the Project

Press **F5** in Visual Studio or:

```bash
cd SmartComplaint.API
dotnet run
```

Once running, open your browser:

| URL | Description |
|---|---|
| `https://localhost:{port}/swagger` | Swagger UI — test all endpoints |
| `https://localhost:{port}/hangfire` | Hangfire Dashboard — background jobs |

### Using Swagger with JWT

1. Call `POST /api/auth/login` with admin credentials
2. Copy the `accessToken` from the response
3. Click **Authorize 🔒** button at the top of Swagger UI
4. Enter: `Bearer {your_access_token}`
5. Click **Authorize** → now all protected endpoints are accessible

---

## 🔑 Default Admin Credentials

After running the project for the first time, a default admin is seeded:

```
Email:    admin@smartcomplaint.com
Password: Admin@123
Role:     Admin
```

> ⚠️ Change this password immediately in a production environment.

---

## ⏱️ Background Jobs

Hangfire runs a **SLA Monitor Job every 15 minutes** automatically.

You can also trigger it manually from the Hangfire Dashboard:
1. Go to `https://localhost:{port}/hangfire`
2. Click **Recurring Jobs**
3. Click **Trigger Now** on `sla-monitor`

**What the job does:**
- Fetches all `Open` and `InProgress` complaints
- Compares complaint age against SLA policy limits
- On breach: sends email to Admin, creates in-app notification, logs to AuditLogs
- Duplicate breach notifications are prevented (checks before inserting)

---

## 📋 SLA Policy

| Priority | Maximum Resolution Time |
|---|---|
| High | 4 hours |
| Medium | 12 hours |
| Low | 24 hours |

SLA policies are seeded automatically on first run and can be modified in the `SLAPolicies` table.

---

## 📧 Email Events

The system sends HTML emails for the following events:

| Event | Recipient |
|---|---|
| OTP Verification | Registered User |
| Password Reset Link | User who requested it |
| Complaint Created | Complaint owner |
| Complaint Assigned | Assigned Agent |
| Status Changed | Complaint owner |
| SLA Breach Alert | Admin |
| Complaint Resolved | Complaint owner |

---

## 🧪 Running Tests

In Visual Studio:

```
Test → Run All Tests   (or Ctrl + R, A)
```

Current test coverage includes:

| Test | Description |
|---|---|
| `Register_ShouldThrow_WhenEmailAlreadyExists` | Duplicate email check |
| `Register_ShouldSucceed_WhenEmailIsNew` | Successful registration flow |
| `VerifyOtp_ShouldThrow_WhenOtpIsInvalid` | Wrong OTP rejection |
| `VerifyOtp_ShouldThrow_WhenOtpIsExpired` | Expired OTP rejection |
| `Login_ShouldThrow_WhenUserNotFound` | Invalid email check |
| `Login_ShouldThrow_WhenEmailNotVerified` | Unverified account check |

---

## 📁 Project Structure

```
SmartComplaintSystem/
│
├── SmartComplaint.API/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── ComplaintsController.cs
│   │   ├── AttachmentsController.cs
│   │   ├── UsersController.cs
│   │   ├── AssignmentsController.cs
│   │   ├── CommentsController.cs
│   │   ├── NotificationsController.cs
│   │   ├── FeedbackController.cs
│   │   └── DashboardController.cs
│   ├── Middleware/
│   │   └── ExceptionMiddleware.cs
│   ├── wwwroot/uploads/
│   ├── logs/
│   ├── appsettings.json
│   └── Program.cs
│
├── SmartComplaint.Application/
│   ├── DTOs/
│   │   ├── AuthDTOs.cs
│   │   ├── ComplaintDTOs.cs
│   │   ├── AttachmentDTOs.cs
│   │   ├── UserDTOs.cs
│   │   ├── AssignmentDTOs.cs
│   │   ├── CommentDTOs.cs
│   │   ├── NotificationDTOs.cs
│   │   ├── FeedbackDTOs.cs
│   │   └── DashboardDTOs.cs
│   ├── Interfaces/
│   │   ├── IRepository.cs
│   │   ├── IUnitOfWork.cs
│   │   ├── IAuthService.cs
│   │   ├── IEmailService.cs
│   │   ├── IComplaintService.cs
│   │   ├── IAttachmentService.cs
│   │   ├── IUserService.cs
│   │   ├── IAssignmentService.cs
│   │   ├── ICommentService.cs
│   │   ├── INotificationService.cs
│   │   ├── IFeedbackService.cs
│   │   ├── IDashboardService.cs
│   │   ├── ISlaMonitorService.cs
│   │   └── JwtSettings.cs
│   └── Mappings/
│       └── MappingProfile.cs
│
├── SmartComplaint.Domain/
│   ├── Entities/
│   │   ├── BaseEntity.cs
│   │   ├── User.cs
│   │   ├── Category.cs
│   │   ├── Complaint.cs
│   │   ├── ComplaintAssignment.cs
│   │   ├── Comment.cs
│   │   ├── Attachment.cs
│   │   ├── Notification.cs
│   │   ├── ComplaintHistory.cs
│   │   ├── SLAPolicy.cs
│   │   ├── Feedback.cs
│   │   └── AuditLog.cs
│   └── Enums/
│       ├── UserRole.cs
│       └── ComplaintEnums.cs
│
├── SmartComplaint.Infrastructure/
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   └── DbSeeder.cs
│   ├── Migrations/
│   ├── Repositories/
│   │   ├── Repository.cs
│   │   └── UnitOfWork.cs
│   ├── Email/
│   │   └── EmailService.cs
│   └── Services/
│       ├── AuthService.cs
│       ├── ComplaintService.cs
│       ├── AttachmentService.cs
│       ├── UserService.cs
│       ├── AssignmentService.cs
│       ├── CommentService.cs
│       ├── NotificationService.cs
│       ├── FeedbackService.cs
│       ├── DashboardService.cs
│       └── SlaMonitorService.cs
│
└── SmartComplaint.Tests/
    └── AuthServiceTests.cs
```

---

## 🚦 Status Transition Rules

```
Open ──────→ InProgress
InProgress → OnHold
InProgress → Resolved
OnHold ────→ InProgress
Resolved ──→ Closed
Closed ────→ ❌ (terminal state)
```

Any invalid transition returns a `400 Bad Request` with a clear error message.

---

## 📊 Project Stats

| Metric | Value |
|---|---|
| Total Phases | 8 |
| Timeline | 6 Weeks |
| API Endpoints | 50+ |
| Database Tables | 11 |
| Email Events | 7 |
| Background Jobs | 1 (every 15 min) |
| Unit Tests | 6 |
| Architecture | Clean Architecture |

---

## 👤 Author

**Akshit Saini**
Built with ❤️ using ASP.NET Core 8 & Clean Architecture

---

## 📄 License

This project is for educational and portfolio purposes.
