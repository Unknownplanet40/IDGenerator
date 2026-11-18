# Tasks: Student ID Generator (WinUI 3 Rebuild)

## Milestone 1: Core Domain & Tests (`IdGen.Core` + `IdGen.Tests`)
- [x] Task 1.1: Create solution `IdGen.sln` and scaffold projects `IdGen.Core` and `IdGen.Tests` (.NET 8).
  - Acceptance: `dotnet build` succeeds without warnings.
  - Depends on: Approved plan.
- [x] Task 1.2: Implement domain models, entities, and enums (`User`, `Section`, `Setting`, `IdSubmission`, `ResetRequest`, `Notification`).
  - Acceptance: All models defined according to DATA MODEL spec.
  - Depends on: Task 1.1.
- [x] Task 1.3: Implement `PasswordHasher` (PBKDF2-SHA256, 600,000 iterations, 16-byte random salt, constant-time comparison).
  - Acceptance: Passwords hash securely; verification works; constant-time check verified.
  - Depends on: Task 1.1.
- [x] Task 1.4: Implement `AgeCalculator` with leap day (`Feb 29`) and birthday boundary precision.
  - Acceptance: Birthday today, tomorrow, and leap year tests pass.
  - Depends on: Task 1.1.
- [x] Task 1.5: Implement validators for Registration, Login, Sections, and ID Submissions.
  - Acceptance: Required fields, email format, PH phone regex, min 8 chars password, length caps validated.
  - Depends on: Task 1.2.
- [x] Task 1.6: Define service interfaces (`IAuthService`, `IAdminService`, `IStudentService`, `INotificationService`, `ISettingsService`).
  - Acceptance: Contracts support full requirements and future notification plug-ins.
  - Depends on: Task 1.2.
- [x] Task 1.7: Write and pass xUnit tests in `IdGen.Tests` for Core components.
  - Acceptance: All hash, auth errors (A3), age, and validation tests pass (`dotnet test`).
  - Depends on: Tasks 1.2 - 1.6.

## Milestone 2: Data Access & Repositories (`IdGen.Data` + Tests)
- [x] Task 2.1: Scaffold `IdGen.Data` project (.NET 8) with `Microsoft.Data.Sqlite`.
  - Acceptance: Reference added and project builds cleanly.
  - Depends on: Task 1.7.
- [x] Task 2.2: Implement SQLite database connection factory and schema migration (`PRAGMA user_version`, `PRAGMA foreign_keys = ON;`).
  - Acceptance: Tables created with exact columns, constraints, unique indexes, and foreign keys.
  - Depends on: Task 2.1.
- [x] Task 2.3: Implement parameterized repositories (`UserRepository`, `SectionRepository`, `SettingRepository`, `IdSubmissionRepository`, `ResetRequestRepository`, `NotificationRepository`).
  - Acceptance: CRUD operations work with 100% parameterized queries.
  - Depends on: Task 2.2.
- [x] Task 2.4: Implement section deletion guard preventing deletion when referenced in ID submissions.
  - Acceptance: Attempting to delete an assigned section raises clear domain exception.
  - Depends on: Task 2.3.
- [x] Task 2.5: Write and pass repository xUnit tests using in-memory SQLite.
  - Acceptance: All repository, constraint, cascade, and foreign key tests pass (`dotnet test`).
  - Depends on: Tasks 2.2 - 2.4.

## Milestone 3: App Shell, Admin Setup, Auth (`IdGen.App`)
- [x] Task 3.1: Scaffold `IdGen.App` WinUI 3 unpackaged project with MVVM and DI container.
  - Acceptance: App launches with Mica backdrop, navigation shell, and theme support.
  - Depends on: Task 2.5.
- [x] Task 3.2: Implement First-Run Admin Setup view and navigation logic.
  - Acceptance: If no admin exists in DB, forces admin creation (min 8 chars password); no hardcoded defaults.
  - Depends on: Task 3.1.
- [x] Task 3.3: Implement Login view with exact spec error messages (A3) and status gates (A5).
  - Acceptance: "Username is incorrect", "Password is incorrect", "Username and password are incorrect", Pending ("Your account is pending approval"), and Declined ("Your registration was declined") work properly.
  - Depends on: Task 3.2.
- [x] Task 3.4: Implement Student Registration view with inline validation.
  - Acceptance: Creates student in Pending status; full field validations work with inline error display.
  - Depends on: Task 3.3.

## Milestone 4: Admin Screens (`IdGen.App`)
- [x] Task 4.1: Implement Admin Shell and Student Registrations view (list, approve, decline + in-app notification).
  - Acceptance: Admin can approve/decline pending registrations; student receives in-app notification.
  - Depends on: Task 3.4.
- [x] Task 4.2: Implement Year & Section Management view (add, delete, adviser assignment, delete blocking).
  - Acceptance: Sections managed; deleting an in-use section is blocked with clear UI dialog/message.
  - Depends on: Task 4.1.
- [x] Task 4.3: Implement Admin Defaults view (Principal name, School name).
  - Acceptance: Settings saved to DB and reflected in new ID submissions.
  - Depends on: Task 4.1.
- [x] Task 4.4: Implement ID Submissions Review view (list, review, approve, disapprove + in-app notification).
  - Acceptance: Admin approves or disapproves submissions with student notification.
  - Depends on: Task 4.1.
- [x] Task 4.5: Implement Reset Requests view (list, approve, decline + notification).
  - Acceptance: Approving reset request deletes student submission to unlock ID generator.
  - Depends on: Task 4.4.

## Milestone 5: Student Screens (`IdGen.App`)
- [x] Task 5.1: Implement Student Shell and ID Generator form with auto-filled fields and live age calculation.
  - Acceptance: Registration details and defaults auto-filled; age calculated correctly from birth date.
  - Depends on: Task 4.5.
- [x] Task 5.2: Implement custom digital signature drawing Canvas and PNG export.
  - Acceptance: Student can draw signature, clear, and export non-empty PNG bytes.
  - Depends on: Task 5.1.
- [x] Task 5.3: Implement submission confirmation checkbox, submission lock, and InfoBar notification.
  - Acceptance: Submit blocked until checked; submission locks form and sets status to Pending.
  - Depends on: Task 5.2.
- [x] Task 5.4: Implement View ID screen with CR80 front and back card preview and status badges.
  - Acceptance: Aspect ratio 85.6x54 mm preview renders front and back; badges display correct text based on status.
  - Depends on: Task 5.3.
- [x] Task 5.5: Implement Reset ID request submission and status tracking.
  - Acceptance: Student submits reset request; generator unlocks when approved by admin.
  - Depends on: Task 5.4.
- [x] Task 5.6: Implement Student Account Settings (profile view and password change with re-hashing).
  - Acceptance: Profile displayed; password updated with new salt.
  - Depends on: Task 5.1.

## Milestone 6: Polish, Accessibility & Final Verification
- [x] Task 6.1: Keyboard accessibility, tab ordering, and `AutomationProperties.Name` across all views.
  - Acceptance: All interactive elements accessible via keyboard and screen reader names present.
  - Depends on: Tasks 5.1 - 5.6.
- [x] Task 6.2: End-to-end integration and smoke verification of full user flows (Admin + Student).
  - Acceptance: Complete lifecycle from admin setup to student registration, ID generation, reset, and approval passes.
  - Depends on: Task 6.1.
- [x] Task 6.3: Final test run (`dotnet test`), documentation update in `handoff.md`, and project summary.
  - Acceptance: 100% tests pass; clear instructions provided for running the app.
  - Depends on: Task 6.2.
