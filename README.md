# 🎯 Smart Complaint Management System

A **production-ready, enterprise-level** complaint management system built with **ASP.NET Core 8 Web API**, following **Clean Architecture** principles with SQL Server as the database.

---

## 📋 Table of Contents

- [Project Overview](#project-overview)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Features](#features)
- [Prerequisites](#prerequisites)
- [Database Setup](#database-setup)
- [Project Setup](#project-setup)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [API Endpoints](#api-endpoints)
- [Authentication Guide](#authentication-guide)
- [Testing](#testing)
- [Background Jobs](#background-jobs)
- [Logging](#logging)
- [Project Structure](#project-structure)

---

## 📌 Project Overview

Smart Complaint Management System allows users to raise complaints, agents to manage and resolve them, and admins to monitor the entire process — including SLA tracking, analytics dashboards, email notifications, and audit logging.

---

## 🛠️ Tech Stack

| Technology | Purpose |
|---|---|
| ASP.NET Core 8 Web API | Backend Framework |
| SQL Server (SSMS) | Database |
| Entity Framework Core 8 | ORM (Code First) |
| JWT Bearer | Authentication |
| BCrypt.Net | Password Hashing |
| MailKit | Email Service (SMTP) |
| AutoMapper | Entity ↔ DTO Mapping |
| FluentValidation | Request Validation |
| Hangfire | Background Jobs |
| Serilog | Structured Logging |
| Swagger / Swashbuckle | API Documentation |
| AspNetCoreRateLimit | Rate Limiting |
| xUnit + Moq | Unit Testing |

---

## 🏗️ Architecture

Clean Architecture with 5 projects:

```
SmartComplaintSystem/
├── SmartComplaint.API              → Controllers, Middleware, Program.cs
├── SmartComplaint.Application      → Services Interfaces, DTOs, Validators
├── SmartComplaint.Domain           → Entities, Enums, Constants
├── SmartComplaint.Infrastructure   → DbContext, Repositories, Email, Jobs
└── SmartComplaint.Tests            → Unit Tests (xUnit + Moq)
```

### Dependency Flow
```
API → Application → Domain
API → Infrastructure → Application → Domain
```

---

## ✨ Features

- ✅ JWT Authentication with Access + Refresh Tokens
- ✅ Email OTP Registration Verification
- ✅ Forgot / Reset Password via Email Link
- ✅ Role-Based Authorization (User / Agent / Admin)
- ✅ Smart Auto-Priority Detection (keyword scanning)
- ✅ Complaint Status Transition Engine with Validation
- ✅ Complaint History Tracking
- ✅ File Attachment Upload (jpg, png, pdf, docx)
- ✅ Auto Assignment + Round-Robin Logic
- ✅ In-App Notification System
- ✅ Email Notifications for all Events (HTML Templates)
- ✅ Feedback / Rating System (1–5 stars)
- ✅ SLA Monitoring Background Job (every 15 minutes)
- ✅ Admin / Agent / User Analytics Dashboards
- ✅ Reports API with Filters
- ✅ Serilog Logging (Console + File + SQL Server)
- ✅ Audit Logging for all Write Operations
- ✅ Pagination + Filtering on all List APIs
- ✅ Soft Delete on all entities
- ✅ Rate Limiting (60 requests/minute)
- ✅ Global Exception Handling Middleware
- ✅ Swagger UI with JWT Support

---

## ✅ Prerequisites

Make sure the following are installed before setup:

| Tool | Version | Download |
|---|---|---|
| Visual Studio 2022 | 17.x or later | https://visualstudio.microsoft.com |
| .NET SDK | 8.0 | https://dotnet.microsoft.com/download |
| SQL Server | 2019 / 2022 | https://www.microsoft.com/en-us/sql-server |
| SSMS | 19.x or later | https://aka.ms/ssmsfullsetup |

---

## 🗄️ Database Setup

### Step 1: SQL Server running check karo

SSMS kholo aur `localhost` ya `.\SQLEXPRESS` se connect karo.

### Step 2: Migration run karo

Visual Studio mein:

**Tools → NuGet Package Manager → Package Manager Console**

```powershell
# Default Project: SmartComplaint.Infrastructure
Add-Migration InitialCreate -StartupProject SmartComplaint.API
Update-Database -StartupProject SmartComplaint.API
```

### Step 3: Seed Data verify karo

SSMS mein `SmartComplaintDB` database check karo:

```sql
SELECT * FROM Categories;    -- 6 categories honi chahiye
SELECT * FROM SLAPolicies;   -- 3 SLA policies honi chahiye
SELECT * FROM Users;         -- 1 admin user hona chahiye
```

---

## ⚙️ Configuration

### appsettings.json (complete)

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
    "Username": "youremail@gmail.com",
    "Password": "your-app-password",
    "FromName": "Smart Complaint System",
    "FromEmail": "youremail@gmail.com"
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
    "RealIpHeader": "X-Real-IP",
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 60
      }
    ]
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-.txt",
          "rollingInterval": "Day"
        }
      },
      {
        "Name": "MSSqlServer",
        "Args": {
          "connectionString": "Server=.;Database=SmartComplaintDB;Trusted_Connection=True;TrustServerCertificate=True;",
          "tableName": "Logs",
          "autoCreateSqlTable": true
        }
      }
    ]
  }
}
```

### Gmail SMTP Setup (Email ke liye)

1. Gmail account mein jao → **Google Account Settings**
2. **Security** → **2-Step Verification** enable karo
3. **App Passwords** → Select app: `Mail` → Select device: `Windows Computer`
4. Generated 16-character password ko `EmailSettings:Password` mein daalo

---

## 🚀 Running the Application

### Step 1: Solution kholo

```
SmartComplaintSystem.sln → Visual Studio mein open karo
```

### Step 2: Startup Project set karo

Solution Explorer mein `SmartComplaint.API` pe **Right Click → Set as Startup Project**

### Step 3: Run karo

```
F5        →  Debug mode
Ctrl+F5   →  Without debugger
```

### Step 4: Swagger UI

Browser automatically open hoga:
```
https://localhost:{port}/swagger
```

### Step 5: Hangfire Dashboard

```
https://localhost:{port}/hangfire
```

---

## 🔐 Authentication Guide

### 1. Register karo

```http
POST /api/auth/register
Content-Type: application/json

{
  "name": "Your Name",
  "email": "your@email.com",
  "password": "YourPassword@123"
}
```

### 2. OTP Verify karo (email check karo)

```http
POST /api/auth/verify-otp
Content-Type: application/json

{
  "email": "your@email.com",
  "otp": "123456"
}
```

### 3. Login karo

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "your@email.com",
  "password": "YourPassword@123"
}
```

Response mein `accessToken` milega.

### 4. Swagger mein Authorize karo

Swagger UI mein **Authorize 🔒** button dabao:
```
Bearer eyJhbGciOiJIUzI1NiIs...
```

### Default Admin Credentials

```
Email:    admin@smartcomplaint.com
Password: Admin@123
```

---

## 📡 API Endpoints

### Auth `/api/auth`
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/register` | Register new user | ❌ |
| POST | `/verify-otp` | Verify email OTP | ❌ |
| POST | `/login` | Login + get tokens | ❌ |
| POST | `/refresh-token` | Refresh access token | ❌ |
| POST | `/logout` | Invalidate refresh token | ❌ |
| POST | `/forgot-password` | Send reset link | ❌ |
| POST | `/reset-password` | Reset password | ❌ |

### Users `/api/users`
| Method | Endpoint | Description | Role |
|---|---|---|---|
| POST | `/` | Create user | Admin |
| GET | `/` | Get all users | Admin |
| GET | `/{id}` | Get user by ID | Admin |
| PUT | `/{id}` | Update user | Admin |
| DELETE | `/{id}` | Deactivate user | Admin |
| GET | `/agents` | Get all agents | Admin |
| PATCH | `/{id}/role` | Change role | Admin |

### Complaints `/api/complaints`
| Method | Endpoint | Description | Role |
|---|---|---|---|
| POST | `/` | Create complaint | User |
| GET | `/` | Get all complaints | Admin/Agent |
| GET | `/{id}` | Get complaint detail | Any |
| PUT | `/{id}` | Update complaint | Admin |
| DELETE | `/{id}` | Soft delete | Admin |
| GET | `/my` | My complaints | User |
| PATCH | `/{id}/status` | Update status | Admin/Agent |

### Attachments `/api/complaints/{id}/attach`
| Method | Endpoint | Description | Role |
|---|---|---|---|
| POST | `/` | Upload file | Any |
| GET | `/` | Get attachments | Any |
| DELETE | `/{attachmentId}` | Delete attachment | Admin |

### Assignments `/api/assignments`
| Method | Endpoint | Description | Role |
|---|---|---|---|
| POST | `/` | Assign complaint | Admin |
| GET | `/agent/{agentId}` | Agent assignments | Admin/Agent |
| PUT | `/{id}` | Reassign | Admin |

### Comments `/api/comments`
| Method | Endpoint | Description | Role |
|---|---|---|---|
| POST | `/` | Add comment | Any |
| GET | `/{complaintId}` | Get comments | Any |
| DELETE | `/{id}` | Delete comment | Owner/Admin |

### Notifications `/api/notifications`
| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | `/` | My notifications | Any |
| PATCH | `/{id}/read` | Mark as read | Any |
| PATCH | `/read-all` | Mark all read | Any |
| DELETE | `/{id}` | Delete | Any |

### Feedback `/api/feedback`
| Method | Endpoint | Description | Role |
|---|---|---|---|
| POST | `/` | Submit feedback | User |
| GET | `/{complaintId}` | Get feedback | Any |
| GET | `/report` | Average ratings | Admin |

### Dashboard `/api/dashboard`
| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | `/admin` | Admin stats | Admin |
| GET | `/agent` | Agent stats | Admin/Agent |
| GET | `/user` | User stats | Any |
| GET | `/reports` | Filtered reports | Admin |

---

## ⏱️ Background Jobs

Hangfire runs a recurring job every 15 minutes:

**SLA Monitor Job:**
- Fetches all Open/InProgress complaints
- Checks if age exceeds SLA limit:
  - High Priority → 4 hours
  - Medium Priority → 12 hours
  - Low Priority → 24 hours
- On breach: sends admin email + creates notification + logs to AuditLogs

**Manual trigger:**
```
https://localhost:{port}/hangfire → Recurring Jobs → Trigger
```

---

## 🔍 Smart Priority Detection

When creating a complaint, priority is auto-detected from Title + Description:

| Keywords | Priority |
|---|---|
| server down, not working, emergency, critical, outage | 🔴 High |
| slow, intermittent, degraded, delay | 🟡 Medium |
| password, reset, inquiry, question, minor | 🟢 Low |

Manual priority can also be passed in request body.

---

## 📊 Complaint Status Flow

```
Open → InProgress → OnHold → Resolved → Closed
              ↑__________|
```

Invalid transitions are rejected with 400 Bad Request.
Every status change is logged in ComplaintHistory table.

---

## 📝 Logging

Three logging destinations configured via Serilog:

| Destination | Location | Purpose |
|---|---|---|
| Console | Terminal output | Development debugging |
| File | `logs/log-YYYYMMDD.txt` | Daily rolling file |
| SQL Server | `Logs` table in DB | Persistent queryable logs |

Events logged:
- Every HTTP request (method, path, status, time)
- Auth attempts (success/failure)
- Complaint created/assigned/resolved
- Email sent/failed
- SLA breaches
- Unhandled exceptions (with stack trace)

---

## 🧪 Testing

### Unit Tests run karo

```
Visual Studio → Test → Run All Tests
Shortcut: Ctrl+R, A
```

Tests cover:
- Register — duplicate email validation
- Register — successful registration + OTP email
- OTP Verify — invalid OTP
- OTP Verify — expired OTP
- Login — user not found
- Login — unverified email

---

## 🗃️ Database Tables (11 Tables)

| Table | Description |
|---|---|
| Users | User accounts with roles |
| Categories | Complaint categories |
| Complaints | Core complaint data |
| ComplaintAssignments | Agent assignments |
| Comments | Complaint comments |
| Attachments | File uploads |
| Notifications | In-app notifications |
| ComplaintHistory | Status change log |
| SLAPolicies | SLA time limits |
| Feedbacks | User ratings |
| AuditLogs | System audit trail |

---

## 📁 Project Structure

```
SmartComplaintSystem/
│
├── SmartComplaint.API/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── UsersController.cs
│   │   ├── ComplaintsController.cs
│   │   ├── AttachmentsController.cs
│   │   ├── AssignmentsController.cs
│   │   ├── CommentsController.cs
│   │   ├── NotificationsController.cs
│   │   ├── FeedbackController.cs
│   │   └── DashboardController.cs
│   ├── Middleware/
│   │   └── ExceptionMiddleware.cs
│   ├── wwwroot/uploads/
│   ├── logs/
│   ├── Program.cs
│   └── appsettings.json
│
├── SmartComplaint.Application/
│   ├── DTOs/
│   ├── Interfaces/
│   └── Mappings/
│       └── MappingProfile.cs
│
├── SmartComplaint.Domain/
│   ├── Entities/     (12 files)
│   └── Enums/        (2 files)
│
├── SmartComplaint.Infrastructure/
│   ├── Data/
│   ├── Repositories/
│   ├── Email/
│   ├── Services/     (10 services)
│   └── Migrations/
│
└── SmartComplaint.Tests/
    └── AuthServiceTests.cs
```

---

## 📊 Project Stats

| Item | Count |
|---|---|
| API Endpoints | 50+ |
| Database Tables | 11 |
| Projects in Solution | 5 |
| Email Event Types | 7 |
| Background Jobs | 1 (every 15 min) |
| Unit Tests | 6 |
| Architecture | Clean Architecture |
| Total Development Time | 6 Weeks |

---

## 👨‍💻 Developer

**Akshit**
Built with ❤️ using ASP.NET Core 8 Clean Architecture

---

## 📄 License

This project is for educational and portfolio purposes.
