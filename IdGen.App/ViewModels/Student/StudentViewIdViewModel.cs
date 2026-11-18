using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IdGen.App.Services;
using IdGen.Core.Enums;
using IdGen.Core.Models;
using IdGen.Core.Models.Template;
using IdGen.Core.Services;
using IdGen.Core.Validation;
using IdGen.Data.Repositories;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

namespace IdGen.App.ViewModels.Student;

public partial class StudentViewIdViewModel : ObservableObject
{
    private readonly IStudentService _studentService;
    private readonly IAdminService _adminService;
    private readonly ITemplateService _templateService;
    private readonly ITemplateAssetRepository _templateAssetRepository;
    private readonly IUserSession _userSession;

    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private bool _hasSubmission;
    [ObservableProperty] private bool _isApproved;
    [ObservableProperty] private string _studentNo = string.Empty;
    [ObservableProperty] private string _fullName = string.Empty;
    [ObservableProperty] private string _statusBadgeText = string.Empty;
    [ObservableProperty] private InfoBarSeverity _statusBadgeSeverity = InfoBarSeverity.Informational;

    // Template Definitions
    [ObservableProperty] private string _orientation = "Portrait";
    [ObservableProperty] private double _canvasWidth = TemplateConstants.StandardPortraitWidth;
    [ObservableProperty] private double _canvasHeight = TemplateConstants.StandardPortraitHeight;

    public TemplateSideDefinition FrontSide { get; private set; } = new();
    public TemplateSideDefinition BackSide { get; private set; } = new();

    // Backgrounds
    [ObservableProperty] private string _frontBackgroundColor = "#FFFFFF";
    [ObservableProperty] private string _frontBackgroundFit = "Cover";
    [ObservableProperty] private BitmapImage? _frontBackgroundImage;
    [ObservableProperty] private bool _hasFrontBackgroundImage;

    [ObservableProperty] private string _backBackgroundColor = "#FFFFFF";
    [ObservableProperty] private string _backBackgroundFit = "Cover";
    [ObservableProperty] private BitmapImage? _backBackgroundImage;
    [ObservableProperty] private bool _hasBackBackgroundImage;

    // Assets
    [ObservableProperty] private BitmapImage? _schoolLogoImage;
    [ObservableProperty] private BitmapImage? _studentSignatureImage;
    [ObservableProperty] private BitmapImage? _studentPhotoImage;

    // Resolved field values dictionary
    public Dictionary<string, string> FieldValues { get; } = new(StringComparer.OrdinalIgnoreCase);

    public StudentViewIdViewModel(
        IStudentService studentService,
        IAdminService adminService,
        ITemplateService templateService,
        ITemplateAssetRepository templateAssetRepository,
        IUserSession userSession)
    {
        _studentService = studentService;
        _adminService = adminService;
        _templateService = templateService;
        _templateAssetRepository = templateAssetRepository;
        _userSession = userSession;
    }

    [RelayCommand]
    public async Task LoadSubmissionAsync()
    {
        IsLoading = true;
        try
        {
            var user = _userSession.CurrentUser;
            if (user == null)
            {
                HasSubmission = false;
                return;
            }

            var submission = await _studentService.GetSubmissionAsync(user.Id);
            if (submission == null)
            {
                HasSubmission = false;
                return;
            }

            HasSubmission = true;

            // Load Template
            var template = await _templateService.GetTemplateAsync();
            Orientation = template.Orientation;
            if (Orientation.Equals("Landscape", StringComparison.OrdinalIgnoreCase))
            {
                CanvasWidth = TemplateConstants.StandardLandscapeWidth;
                CanvasHeight = TemplateConstants.StandardLandscapeHeight;
            }
            else
            {
                CanvasWidth = TemplateConstants.StandardPortraitWidth;
                CanvasHeight = TemplateConstants.StandardPortraitHeight;
            }

            FrontSide = TemplateValidator.ParseOrDefault(template.FrontJson, isFront: true);
            BackSide = TemplateValidator.ParseOrDefault(template.BackJson, isFront: false);

            FrontBackgroundColor = FrontSide.Background.Color;
            FrontBackgroundFit = FrontSide.Background.Fit;
            if (FrontSide.Background.AssetId.HasValue)
            {
                var bgAsset = await _templateAssetRepository.GetByIdAsync(FrontSide.Background.AssetId.Value);
                if (bgAsset != null && bgAsset.Data.Length > 0)
                {
                    FrontBackgroundImage = await CreateBitmapFromBytesAsync(bgAsset.Data);
                    HasFrontBackgroundImage = true;
                }
            }

            BackBackgroundColor = BackSide.Background.Color;
            BackBackgroundFit = BackSide.Background.Fit;
            if (BackSide.Background.AssetId.HasValue)
            {
                var bgAsset = await _templateAssetRepository.GetByIdAsync(BackSide.Background.AssetId.Value);
                if (bgAsset != null && bgAsset.Data.Length > 0)
                {
                    BackBackgroundImage = await CreateBitmapFromBytesAsync(bgAsset.Data);
                    HasBackBackgroundImage = true;
                }
            }

            // Load School Logo
            var logoAsset = await _templateAssetRepository.GetActiveAssetByKindAsync("SchoolLogo");
            if (logoAsset != null && logoAsset.Data.Length > 0)
            {
                SchoolLogoImage = await CreateBitmapFromBytesAsync(logoAsset.Data);
            }

            // Load Signature
            if (submission.SignaturePng != null && submission.SignaturePng.Length > 0)
            {
                StudentSignatureImage = await CreateBitmapFromBytesAsync(submission.SignaturePng);
            }

            // Load Student Photo
            if (submission.PhotoPng != null && submission.PhotoPng.Length > 0)
            {
                StudentPhotoImage = await CreateBitmapFromBytesAsync(submission.PhotoPng);
            }

            // Populate Field Values from submission snapshots
            StudentNo = user.StudentNo ?? "";
            FullName = $"{user.LastName}, {user.FirstName} {user.MiddleName}".Trim();
            IsApproved = submission.Status == SubmissionStatus.Approved;

            FieldValues.Clear();
            FieldValues["student_no"] = StudentNo;
            FieldValues["full_name"] = FullName;
            FieldValues["first_name"] = user.FirstName;
            FieldValues["last_name"] = user.LastName;
            FieldValues["middle_name"] = user.MiddleName ?? "";
            FieldValues["gender"] = submission.Gender;
            FieldValues["birth_date"] = submission.BirthDate.ToString("MMMM dd, yyyy");
            FieldValues["age"] = AgeCalculator.Calculate(submission.BirthDate, DateOnly.FromDateTime(DateTime.Today)).ToString();
            FieldValues["blood_type"] = string.IsNullOrWhiteSpace(submission.BloodType) ? "N/A" : submission.BloodType;
            FieldValues["address"] = submission.Address;
            FieldValues["guardian_name"] = submission.GuardianName;
            FieldValues["guardian_phone"] = submission.GuardianPhone;

            FieldValues["school_name"] = submission.SchoolSnapshot;
            FieldValues["campus_name"] = submission.CampusSnapshot ?? "";
            FieldValues["principal_name"] = submission.PrincipalSnapshot;
            FieldValues["principal_title"] = submission.PrincipalTitleSnapshot ?? "";
            FieldValues["semester"] = TemplateConstants.FormatSemester(submission.SemesterSnapshot);
            FieldValues["school_year"] = submission.SchoolYearSnapshot ?? "";
            FieldValues["course"] = submission.CourseSnapshot ?? "";
            FieldValues["adviser"] = submission.AdviserSnapshot;

            // Load Section Name
            var sections = await _adminService.GetAllSectionsAsync();
            var sec = sections.FirstOrDefault(s => s.Id == submission.SectionId);
            FieldValues["year_level"] = sec?.YearLevel ?? "";
            FieldValues["section_name"] = sec?.SectionName ?? "";

            switch (submission.Status)
            {
                case SubmissionStatus.Approved:
                    StatusBadgeText = "Ready to claim";
                    StatusBadgeSeverity = InfoBarSeverity.Success;
                    break;
                case SubmissionStatus.Disapproved:
                    StatusBadgeText = "Coordinate with admin";
                    StatusBadgeSeverity = InfoBarSeverity.Error;
                    break;
                case SubmissionStatus.Pending:
                default:
                    StatusBadgeText = "Need to wait";
                    StatusBadgeSeverity = InfoBarSeverity.Warning;
                    break;
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    public string ResolveText(TemplateElementDefinition el)
    {
        if (el.Type == "StaticText")
        {
            return el.StaticText ?? string.Empty;
        }

        if (el.Type == "DynamicText" && !string.IsNullOrWhiteSpace(el.FieldKey))
        {
            return FieldValues.GetValueOrDefault(el.FieldKey, $"[{el.FieldKey}]");
        }

        return string.Empty;
    }

    private static async Task<BitmapImage?> CreateBitmapFromBytesAsync(byte[]? bytes)
    {
        if (bytes == null || bytes.Length == 0) return null;
        using var stream = new InMemoryRandomAccessStream();
        await stream.WriteAsync(bytes.AsBuffer());
        stream.Seek(0);

        var bmp = new BitmapImage();
        await bmp.SetSourceAsync(stream);
        return bmp;
    }
}
