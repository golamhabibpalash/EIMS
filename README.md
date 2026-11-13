# EIMS - Education Institute Management System

A comprehensive school/institute management system built with .NET 8.0.

## Tech Stack

- **Framework**: ASP.NET Core 8.0 MVC (Controllers + Razor Views)
- **ORM**: Entity Framework Core 7.0
- **Database**: MS SQL Server or PostgreSQL (configurable)
- **Authentication**: ASP.NET Core Identity (claim-based authorization)
- **Reports**: RDLC / ReportViewer
- **Background Jobs**: Hangfire
- **Logging**: Serilog (SQL Server sink)
- **SMS**: GreenWeb SMS API integration
- **Email**: SMTP (Gmail)
- **Image Processing**: SixLabors.ImageSharp

## Project Structure

```
SMS/
├── SMS_App/          # Main web application (Controllers, Views, ViewModels)
├── SMS.BLL/          # Business Logic Layer (Manager classes)
├── SMS.DAL/          # Data Access Layer (Repository pattern)
├── SMS.DB/           # EF Core DbContext & migrations
├── SMS.Entities/     # Domain models, enums, ViewModels
├── SMS.Frameworks/   # Shared utilities, tag helpers, encryption
└── Resources/        # Application resources
```

## Key Features

### Student Management
- Student registration and profile management
- Academic session management
- Class and section assignments
- Student fee allocation and payment tracking
- Document management (attachments)

### Academic Management
- Class routines and schedules
- Subject enrollment
- Academic exams (groups, types, details)
- Exam results and grading
- Rank calculation

### Financial Management
- Fee heads configuration
- Student fee allocation
- Payment collection and receipts
- Daily payment collection reports

### Employee Management
- Employee profiles
- Designations and types
- Subject-teacher mapping
- Attendance tracking

### Communication
- SMS notifications
- Email alerts
- Push notifications

### Reports
- Student reports (admit cards, marksheets)
- Payment receipts
- Attendance reports
- Custom dynamic reports

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- MS SQL Server 2019+
- Visual Studio 2022 or VS Code

### Configuration

1. Copy `appsettings.Development.json` or `appsettings.json` and configure:
   - Database connection string
   - SMTP settings for email
   - SMS API credentials
   - Serilog logging settings

2. Run database migrations:
   ```bash
   cd SMS/SMS_App
   dotnet ef database update
   ```

3. Build and run:
   ```bash
   dotnet build
   dotnet run
   ```

### Database Configuration

Supports **PostgreSQL** and **SQL Server**, selected via the `DatabaseProvider` setting in `appsettings.json`:

```json
"DatabaseProvider": "PostgreSQL"  // or "SqlServer"
```

When using PostgreSQL, the `DefaultConnection` is used as plain text. When using SQL Server, it's AES-decrypted using keys from environment variables `AES_KEY` and `AES_IV`.

Multiple pre-configured connection string profiles (all SQL Server, encrypted):
- `DefaultConnection` - Production (plain text for PostgreSQL)
- `Mac_Docker_DefaultConnection` - Docker on Mac
- `Local_Desktop_DefaultConnection` - Local desktop
- `Noble_DefaultConnection` - Noble server
- `Momitun_DefaultConnection` - Momitun server
- `MCPS_DefaultConnection` - MCPS server

## NuGet Packages

- AutoMapper - Object mapping
- Hangfire - Background job processing
- RazorLight - Template rendering
- SixLabors.ImageSharp - Image processing
- AspNetCore.Reporting - Report generation
- NodaTime - Date/time handling

## Database Migrations

The project uses EF Core migrations for database schema management. Key migrations include:
- Academic session management
- Student fee allocation
- Exam management system
- Subject enrollment
- Attendance tracking
- Bus configuration
- Notification system

## License

Private - All rights reserved
