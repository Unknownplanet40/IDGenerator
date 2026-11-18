# Project Memory
<!-- Append-only. Never edit history. -->

## YYYY-MM-DD Title
- [Decision] ...
- [Rejected] ...

## 2026-11-18 Student ID Generator WinUI 3 Rebuild
- [Decision] Architecture locked to .NET 8, C# 12, WinUI 3 (Windows App SDK unpackaged), SQLite via Microsoft.Data.Sqlite with 100% parameterized SQL.
- [Decision] Password hashing: PBKDF2-SHA256, 600,000 iterations, 16-byte random salt, constant-time compare with CryptographicOperations.FixedTimeEquals.
- [Decision] Digital signature implemented via custom pointer-drawing Canvas and RenderTargetBitmap PNG export to guarantee compatibility without Windows App SDK ink dependencies.
- [Decision] Full accessibility with AutomationProperties.Name and keyboard tab indices on every input.

## 2026-11-18 UI Architecture & Table Polish
- [Decision] Set `AlwaysShowHeader="False"` and `IsBackButtonVisible="Collapsed"` on WinUI 3 NavigationViews to eliminate header duplication and back arrow clutter.
- [Decision] Replace per-cell label stacks with dedicated header bars and star-sized columns across all tabular views to match the B2B Dashboard standard.
- [Decision] Set default window launch dimension to 1280x800 for optimal desktop breathing room.

## 2026-11-18 Responsive Full-Width Layout
- [Decision] Set outer containers to `HorizontalAlignment="Stretch"` and disable unconstrained horizontal scrolling on `ScrollViewer` so star (`*`) column widths calculate against the actual window viewport and adapt dynamically on resize.
- [Decision] Balanced table column allocations across all Admin views so the right-side ACTIONS column stays in view without clipping.

## 2026-11-18 View Lifecycle & ViewModel Assignment Order
- [Decision] Always resolve and assign `ViewModel` from DI *before* calling `InitializeComponent()` in code-behind classes. This prevents immediate XAML property triggers (such as `IsChecked="True"` firing `Checked` handlers) from hitting a null `ViewModel` during initial XAML inflation.
- [Decision] Add null checks (`if (ViewModel != null)`) to all XAML event handlers in code-behind as an additional safeguard against early invocation during control tree building.

## 2026-11-18 Visibility Converter Binding Fix
- [Decision] Never bind `UIElement.Visibility` to `IntToBoolConverter`. Use `StringToVisibilityConverter` for string properties (such as inline error messages) so null/empty converts to `Visibility.Collapsed` and non-empty converts to `Visibility.Visible` without runtime cast exceptions.

## 2026-11-18 Addendum 1 & 2 - Template Designer & Settings Architecture
- [Decision] Built pure C# 12 zero-dependency Code 128 barcode encoder with automatic switching between Subsets B & C, modulo 103 checksum, and quiet zone generation. Rendered directly via standard XAML rectangles.
- [Decision] Fixed whitelist for fonts (10 families) and dynamic fields (22 snapshot and settings keys) with strict schema validation and fallback to reference layout upon parse errors.
- [Decision] Template z-order is determined strictly by element order in the JSON list; layering buttons (Bring forward, Send backward, Bring to front, Send to back) manipulate list indices with undo/redo snapshot tracking.
- [Decision] Dynamic CR80 card renderer in Student View ID resolves field values strictly from submit-time immutable snapshot columns to prevent drift when admin settings or section names change later.

## 2026-11-18 XAML Resource Key Normalization
- [Decision] Register both `InverseBoolToVisibilityConverter` and `InvertedBoolToVisibilityConverter` keys in `App.xaml` to prevent runtime `COMException 0x80004005` on pages invoking either naming convention.

## 2026-11-18 Color-to-Brush Converter Fix
- [Decision] Implemented `HexToBrushConverter` in `CommonConverters.cs` and bound it to all `Background` properties in `AdminTemplateDesignerPage` and `StudentViewIdPage`. Binding `StringToVisibilityConverter` to `Border.Background` caused runtime `InvalidCastException` when attempting to cast `Visibility` to `Brush`.

## 2026-11-18 Zoom TextBlock Converter Fix & DefaultButtonStyle Safety
- [Decision] Replaced invalid `IntToBoolConverter` on `TextBlock.Text` (which caused runtime `InvalidCastException: Unable to cast object of type 'System.Boolean' to type 'System.String'`) with `FormatZoom` helper method.
- [Decision] Replaced lookup of non-existent resource `DefaultButtonStyle` in `UpdateSideButtons` with safe `btn.ClearValue(Button.StyleProperty)`.

## 2026-11-18 Fit to Screen & Interactive Element Resizing Engine
- [Decision] Implemented automatic "Fit to Screen" calculation and toolbar button `Fit` adjusting zoom dynamically to viewport dimensions on load, window resize, and orientation toggle.
- [Decision] Added collapsible Tools and Inspector panels (`Toggle Tools`, `Toggle Inspector`) to maximize card workbench breathing room on any screen resolution.
- [Decision] Implemented 8-point interactive on-canvas resize handle adorner system (NW, N, NE, E, SE, S, SW, W) with bidirectional resize cursors, live element re-rendering, clamped canvas boundaries, property inspector sync, and undo/redo snapshot recording.

## 2026-11-18 Template Responsiveness, Student Profile Photo, and Submission Card Preview
- [Decision] Wrapped Admin Template Designer top action bar in horizontal ScrollViewer to prevent button cut-off on narrower viewports.
- [Decision] Expanded FitToScreen vertical safety margin (to 100px) so portrait card bottom edge and barcode are completely visible with breathing room.
- [Decision] Added dual-mode Student Profile Picture selection (Upload file picker and Capture from webcam via Windows.Media.Capture with graceful fallback) with 4MB and image signature validation.
- [Decision] Registered PhotoSource on TemplateElementView and wired live photo decoding into Student View ID card generator and Admin Review dialog.
- [Decision] Rebalanced Admin Submissions table columns (expanded ACTIONS to 250px) to prevent button clipping, bound Student Column to StudentNoDisplay and StudentNameDisplay, and added Preview button launching a Front/Back live CR80 card review dialog with direct Approve/Disapprove actions.

## 2026-11-18 Schema Migration Version 3 & Async Void Lifecycle Guards
- [Decision] Bumped schema version to 3 with `ApplyVersion3` migration to guarantee `photo_png` column exists on previously migrated version 2 databases (`SQLite Error 1: 'no such column: photo_png'`).
- [Decision] Wrapped all `Page_Loaded` async event handlers in try-catch logging to `crash.log` to prevent unhandled WinUI 3 `0xc000027b` process termination.

## 2026-11-18 Admin Reset Requests Lifecycle & Pending Filter
- [Decision] Defaulted Reset Requests review table to pending items only (`GetPendingResetRequestsAsync`); resolved items immediately vacate the pending list upon approval or rejection.
- [Decision] Added "Pending" and "All History" filter toggles to Reset Requests view; resolved items in history show a "Resolved" badge with action buttons hidden.
- [Decision] Mapped `student_no`, `first_name`, `last_name`, and `middle_name` on reset requests to display student number and full name instead of raw integer `UserId`.

## 2026-11-18 Barcode Cutout Fix & System Users Directory
- [Decision] Corrected `RenderBarcode` height budgeting so `barsHeight + textHeight` strictly equals `Element.Height`, eliminating human-readable number overflow clipping when barcode elements sit on card boundaries.
- [Decision] Expanded Admin Submissions ACTIONS column to 290px to prevent button cutoff.
- [Decision] Added "All Users" navigation page (`AdminUsersPage`) under Admin Portal displaying directory of all system accounts (Administrators and Students) with live search and Role/Status filtering.

## 2026-11-18 Year & Section Seeding, Fictitious Defaults, and Default Background Template
- [Decision] Embedded user-supplied background image as `default_front_bg.jpg` in assembly resources and linked it as the active default front template background.
- [Decision] Seeded 17 realistic Year & Sections across BSIT, BSCS, BSEMC, and BSIS with assigned advisers via `DatabaseSeeder.cs`.
- [Decision] Replaced real institution and signatory placeholders with made-up names ("Apex Institute of Science & Technology", "Metropolitan Campus", "Dr. Eleanor M. Vance", University Registrar).
- [Decision] Standardized active designer layout in `TemplateDefaults.cs` as the system reference template.

## 2026-11-18 Semester Text Formatting Guarantee
- [Decision] Implemented `TemplateConstants.FormatSemester` ensuring any semester text ("First", "Second", "Summer") always appends "Semester" (e.g. "First Semester") for display on cards and forms without duplicating existing suffixes.
- [Decision] Integrated automatic semester formatting across `TemplateElementView` text rendering, `StudentViewIdViewModel`, `AdminSubmissionsPage` review visualizer, `AdminTemplateDesignerViewModel`, and `StudentGeneratorViewModel`.
- [Decision] Updated `InputValidators.IsValidSemester` to accept both bare names ("First") and suffixed titles ("First Semester") case-insensitively.

## 2026-11-18 ID Card Hardware Printing & 300 DPI Export Engine
- [Decision] Implemented native Win32 GDI printing pipeline (`NativePrintHelper.cs`) via P/Invoke (`comdlg32.dll` `PrintDlg`, `gdi32.dll` `StartDoc`/`StartPage`/`StretchDIBits`) delivering pixel-perfect CR80 (85.6 mm × 54.0 mm) physical hardware printing.
- [Decision] Directly supports all dedicated ID card printers (Zebra, Fargo, Evolis, Magicard) with automatic 1:1 marginless card mapping, and centers cards true-to-scale on standard office paper printers.
- [Decision] Added lossless 300 DPI PNG export (`CardExportHelper.cs`) saving high-resolution files (`{student_no}_ID_Front_CR80.png` and `{student_no}_ID_Back_CR80.png`).
- [Decision] Built `PrintCardOptionDialog.xaml` enabling user/admin selection of card sides (Both Sides / Duplex, Front Side Only, Back Side Only) and target mode (Print to Printer vs Export PNG).
- [Decision] Integrated printing into Student View ID page (for approved student cards), Admin ID Card Review dialog, Admin Submissions table ACTIONS column, and Admin "Batch Print Approved" toolbar button.
