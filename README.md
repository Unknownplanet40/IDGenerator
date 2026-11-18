# Student ID Generator

Native Windows 11 desktop application for institutional student identification card generation, template design, lifecycle management, and physical card printing.

Built with WinUI 3 (Windows App SDK), C# 12, .NET 8, and SQLite.

---

## Overview

Student ID Generator provides an end-to-end platform for schools and universities to design custom CR80 identification cards, manage student submissions, and produce cards using dedicated ID card printers or standard office printers.

The solution is architected as an unpackaged native desktop application following Clean Architecture principles across four projects:
- `IdGen.Core`: Domain models, entities, validation rules, password hashing, and zero-dependency Code 128 barcode encoder.
- `IdGen.Data`: SQLite database access, schema migrations, repositories, default reference templates, and data seeding.
- `IdGen.App`: Native WinUI 3 desktop user interface with Mica backdrop, MVVM architecture, hardware printing engine, and interactive template designer.
- `IdGen.Tests`: Comprehensive xUnit test suite covering all domains, repositories, security hashing, validators, and encoders.

---

## Key Features

### 1. ID Card Hardware Printing & 300 DPI Export Engine
- Native Win32 GDI printing pipeline (`NativePrintHelper.cs`) using `comdlg32.dll` `PrintDlg` and `gdi32.dll`.
- Direct hardware compatibility with professional ID card printers (Zebra, Fargo, Evolis, Magicard) with 1:1 edge mapping.
- Centered true-to-scale CR80 physical dimensions (85.6 mm × 54.0 mm / 3.375" × 2.125") when printing to standard office paper printers (Letter/A4).
- Lossless 300 DPI PNG image export (`CardExportHelper.cs`) embedding print resolution metadata.
- Card side selection dialog allowing selection of:
  - Both Sides (Duplex / 2-Page: Page 1 Front, Page 2 Back)
  - Front Side Only
  - Back Side Only
- Target actions: Direct print to printer or export to PNG images.
- Student portal access: Students can print or export their official card once approved.
- Admin portal access:
  - Single-card print/export from the live ID Card Review dialog.
  - Quick print button in the submissions table ACTIONS column.
  - Batch print / export button in the table header to output all approved student cards in one job.

### 2. Interactive ID Template Designer
- Dual-sided design canvas supporting CR80 Portrait (638 × 1011 px) and Landscape (1011 × 638 px) formats.
- 8-point interactive on-canvas resize adorner system (NW, N, NE, E, SE, S, SW, W) with bidirectional cursors and clamped boundaries.
- Drag-and-drop element repositioning with real-time property inspector synchronization.
- Element types:
  - Dynamic fields: Student number, full name, course, year level, section, adviser, gender, birth date, age, blood type, address, emergency contact, school name, campus, signatory, academic semester, and school year.
  - Static text blocks with customizable font family, font size, weight, italic style, alignment, and color.
  - Geometric shapes: Filled and bordered rectangles with corner radii, lines, and dividers.
  - Zero-dependency Code 128 barcode widget with configurable height, human-readable label toggle, bar color, and quiet zone padding.
  - Institutional logo and student 2x2 photo placeholders.
  - Digital signature placement widget.
- Full layering and z-ordering controls: Bring Forward, Send Backward, Bring to Front, Send to Back.
- Lock/unlock toggle preventing accidental modification of finalized background elements.
- Infinite Undo/Redo stack tracking canvas modifications.
- High-resolution background image upload with density validation warning for images below 300 DPI.
- Whitelisted typography: Segoe UI, Arial, Times New Roman, Georgia, Calibri, Cambria, Verdana, Tahoma, Trebuchet MS, Impact.
- Embedded reference template and "Reset to Reference Layout" action.

### 3. Institutional Administration Portal
- First-Run Setup: Automatic detection of fresh installations prompting creation of the root administrator account.
- Student Registrations: Queue management for pending student account sign-ups with approve and decline workflows.
- Year & Section Management:
  - Course-specific section management across programs (BSIT, BSCS, BSEMC, BSIS).
  - Assigned faculty adviser management.
  - Safe deletion guards preventing removal of sections referenced in active student submissions.
- School Defaults Manager:
  - 3-card layout managing institutional identity (School name, campus name, institutional logo upload).
  - Institutional signatory configuration (Signatory name and title).
  - Academic term defaults (Semester and academic year).
  - Semester formatting guarantee: Automatic formatting ensuring semester values always display with "Semester" appended.
- ID Submissions Review:
  - Comprehensive submissions queue with live status badges (Pending, Approved, Disapproved).
  - Live ID Card Review modal rendering front and back CR80 cards with live student photo, digital signature, and barcode.
  - In-dialog approval, disapproval, and print/export actions.
- Reset ID Requests:
  - Dedicated lifecycle view for student resubmission requests.
  - Pending filter toggle and complete audit history with status badges.
- All Users Directory:
  - Centralized user table displaying all system accounts.
  - Multi-field live search (name, student number, username, email, phone).
  - Role filter (All, Admin, Student) and Status filter (All, Approved, Pending, Declined).

### 4. Student Portal
- ID Generator Form:
  - Auto-populated registration defaults and read-only institutional snapshots.
  - Real-time age calculation from birth date with boundary and leap year precision.
  - Optional Blood Type selector (A+, A-, B+, B-, AB+, AB-, O+, O-).
  - Dual-mode 2x2 Profile Photo: File upload from disk or live webcam capture via Windows Media Capture with image signature verification and 4MB size caps.
  - Interactive digital signature drawing canvas with clear and undo capabilities.
  - Submission confirmation lock preventing accidental multi-submissions.
- View ID Screen:
  - Live CR80 Front and Back ID card rendering resolving the active institutional template and student photo.
  - Dynamic status indicator (Need to wait, Ready to claim, Coordinate with admin).
  - Physical hardware print and 300 DPI PNG export for approved cards.
- Reset ID Requests:
  - Self-service reset request form with reason explanation.
  - Real-time status tracking until administrator resolution.
- Student Account Settings: Profile details view and secure password change.

---

## Security & Data Integrity

- Password Hashing: PBKDF2 with SHA-256, 600,000 iterations, 16-byte cryptographically secure random salt, and constant-time verification (`CryptographicOperations.FixedTimeEquals`).
- Zero SQL Injection: 100% parameterized SQLite queries via `Microsoft.Data.Sqlite`.
- Foreign Key Integrity: SQLite foreign key enforcement enabled (`PRAGMA foreign_keys = ON;`).
- Immutable Audit Snapshots: ID submissions capture immutable snapshot columns (school name, principal, signatory title, adviser, course, semester, school year) at the moment of submission to prevent historical record drift when administrative defaults change.
- Memory Safety: проект contains no unmanaged memory leaks; native GDI handles (`hDC`, `hDevMode`, `hDevNames`) are strictly freed in `finally` blocks.

---

## System Requirements

### For Development & Building
- Windows 10 (version 1809 / build 17763 or higher) or Windows 11.
- .NET 8 SDK (version 8.0.100 or higher).
- Visual Studio 2022 (v17.8+) with the ".NET Desktop Development" workload or the standalone .NET 8 CLI.

### For Running
- Windows 10 (x64) or Windows 11 (x64).
- Windows App SDK runtime (included via self-contained build).
- Any standard Windows-compatible printer, dedicated ID card printer (Zebra, Fargo, Evolis, Magicard), or Microsoft Print to PDF.

---

## Getting Started

### 1. Clone the Repository
```powershell
git clone <repository-url>
cd ID-gen
```

### 2. Run Automated Test Suite
Verify that all 162 unit tests pass:
```powershell
$env:DOTNET_ROOT="C:\Program Files\dotnet"
& "C:\Program Files\dotnet\dotnet.exe" test IdGen.Tests/IdGen.Tests.csproj
```

### 3. Build the Application
Compile the native x64 binary:
```powershell
$env:DOTNET_ROOT="C:\Program Files\dotnet"
& "C:\Program Files\dotnet\dotnet.exe" build IdGen.App/IdGen.App.csproj -p:Platform=x64
```

### 4. Launch the Application
Run the desktop app using the .NET CLI:
```powershell
$env:DOTNET_ROOT="C:\Program Files\dotnet"
& "C:\Program Files\dotnet\dotnet.exe" run --project IdGen.App/IdGen.App.csproj -p:Platform=x64
```
Or launch the pre-compiled binary directly:
```powershell
.\IdGen.App\bin\x64\Debug\net8.0-windows10.0.26100.0\win-x64\IdGen.App.exe
```

---

## Initial Setup & Credentials

1. On first launch, the application checks if an administrator exists.
2. If no admin account is found, the **First-Run Administrator Setup** screen is presented.
3. Create your master administrator account (minimum 8 characters password).
4. Log in using your created admin credentials.
5. Seed data automatically sets up 17 default academic sections and standard institutional defaults ("Apex Institute of Science & Technology").
6. Students can register accounts via the **Register** link on the login page; registered student accounts require admin approval before first login.

---

## Project Structure

```text
ID-gen/
|-- IdGen.sln                         # Solution configuration
|-- IdGen.Core/                       # Core domain models and logic
|   |-- Constants/                    # Setting keys and system constants
|   |-- Enums/                        # User roles, submission status, semesters
|   |-- Models/                       # Entities (User, Section, IdSubmission, etc.)
|   |   `-- Template/                 # CR80 Template schemas and element definitions
|   |-- Services/                     # Code 128 barcode encoder and interfaces
|   `-- Validation/                   # Input, template, and image validators
|-- IdGen.Data/                       # Data persistence and repositories
|   |-- DatabaseMigrator.cs           # SQLite schema migration manager
|   |-- DatabaseSeeder.cs             # Initial section and defaults seeder
|   |-- Repositories/                 # Parameterized SQLite data access
|   |-- Services/                     # Service implementations and default templates
|   `-- Resources/                    # Embedded background assets
|-- IdGen.App/                        # Native WinUI 3 presentation layer
|   |-- Controls/                     # TemplateElementView and adorner controls
|   |-- Converters/                   # XAML data converters
|   |-- Services/                     # Native GDI print helper, export helper, session
|   |-- ViewModels/                   # MVVM ViewModels (Admin and Student)
|   `-- Views/                        # WinUI 3 XAML pages and dialogs
|-- IdGen.Tests/                      # 162 automated xUnit test cases
|-- docs/                             # Architecture and task documentation
`-- handoff.md                        # Project handoff status and technical log
```

---

## License

This project is licensed under the MIT License.
