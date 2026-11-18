using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IdGen.App.Services;
using IdGen.Core.Constants;
using IdGen.Core.Enums;
using IdGen.Core.Models;
using IdGen.Core.Models.Template;
using IdGen.Core.Services;
using IdGen.Core.Validation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

namespace IdGen.App.ViewModels.Student;

public partial class StudentGeneratorViewModel : ObservableObject
{
    private readonly IStudentService _studentService;
    private readonly IAdminService _adminService;
    private readonly ISettingsService _settingsService;
    private readonly IUserSession _userSession;

    public Func<Task<byte[]?>>? SignatureProvider { get; set; }
    public Action? ClearSignatureAction { get; set; }

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private bool _isLocked;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private InfoBarSeverity _statusSeverity = InfoBarSeverity.Informational;

    // Read-only user registration details
    [ObservableProperty]
    private string _studentNo = string.Empty;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    // School & Academic read-only defaults
    [ObservableProperty]
    private string _schoolName = string.Empty;

    [ObservableProperty]
    private string _campusName = string.Empty;

    [ObservableProperty]
    private string _principalName = string.Empty;

    [ObservableProperty]
    private string _principalTitle = string.Empty;

    [ObservableProperty]
    private string _semester = string.Empty;

    [ObservableProperty]
    private string _schoolYear = string.Empty;

    [ObservableProperty]
    private string _course = string.Empty;

    // Form inputs
    [ObservableProperty]
    private string _gender = "Male";

    [ObservableProperty]
    private DateOnly? _birthDate;

    [ObservableProperty]
    private string _age = "-";

    [ObservableProperty]
    private string? _bloodType = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Section> _sections = new();

    [ObservableProperty]
    private Section? _selectedSection;

    [ObservableProperty]
    private string _adviser = string.Empty;

    [ObservableProperty]
    private string _address = string.Empty;

    [ObservableProperty]
    private string _guardianName = string.Empty;

    [ObservableProperty]
    private string _guardianPhone = string.Empty;

    [ObservableProperty]
    private bool _isAcceptInfoChecked;

    // Inline errors
    [ObservableProperty]
    private string _genderError = string.Empty;

    [ObservableProperty]
    private string _birthDateError = string.Empty;

    [ObservableProperty]
    private string _bloodTypeError = string.Empty;

    [ObservableProperty]
    private string _sectionError = string.Empty;

    [ObservableProperty]
    private string _addressError = string.Empty;

    [ObservableProperty]
    private string _guardianNameError = string.Empty;

    [ObservableProperty]
    private string _guardianPhoneError = string.Empty;

    [ObservableProperty]
    private string _signatureError = string.Empty;

    [ObservableProperty]
    private string _photoError = string.Empty;

    [ObservableProperty]
    private byte[]? _photoBytes;

    [ObservableProperty]
    private BitmapImage? _photoImage;

    [ObservableProperty]
    private bool _hasPhoto;

    [ObservableProperty]
    private string _acceptInfoError = string.Empty;

    public ObservableCollection<string> BloodTypeOptions { get; } = new()
    {
        "", "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"
    };

    public StudentGeneratorViewModel(
        IStudentService studentService,
        IAdminService adminService,
        ISettingsService settingsService,
        IUserSession userSession)
    {
        _studentService = studentService;
        _adminService = adminService;
        _settingsService = settingsService;
        _userSession = userSession;
    }

    partial void OnBirthDateChanged(DateOnly? value)
    {
        if (value.HasValue)
        {
            int calculatedAge = AgeCalculator.Calculate(value.Value, DateOnly.FromDateTime(DateTime.Today));
            Age = calculatedAge >= 0 ? calculatedAge.ToString() : "-";
            BirthDateError = string.Empty;
        }
        else
        {
            Age = "-";
        }
    }

    partial void OnSelectedSectionChanged(Section? value)
    {
        Adviser = value?.Adviser ?? string.Empty;
        Course = value?.Course ?? string.Empty;
        if (value != null)
        {
            SectionError = string.Empty;
        }
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        ClearErrors();

        var user = _userSession.CurrentUser;
        if (user == null)
        {
            IsLoading = false;
            return;
        }

        StudentNo = user.StudentNo ?? string.Empty;
        FullName = $"{user.LastName}, {user.FirstName} {user.MiddleName}".Trim();
        Email = user.Email;
        Phone = user.Phone;

        var (school, campus, principal, title, sem, sy) = await _settingsService.GetAllSettingsAsync();
        SchoolName = school;
        CampusName = campus;
        PrincipalName = principal;
        PrincipalTitle = title;
        Semester = TemplateConstants.FormatSemester(sem);
        SchoolYear = sy;

        var sectionsList = await _adminService.GetAllSectionsAsync();
        Sections = new ObservableCollection<Section>(sectionsList);

        // Check if student already submitted ID
        var submission = await _studentService.GetSubmissionAsync(user.Id);
        if (submission != null)
        {
            IsLocked = true;
            Gender = submission.Gender;
            BirthDate = submission.BirthDate;
            BloodType = submission.BloodType ?? string.Empty;
            Address = submission.Address;
            GuardianName = submission.GuardianName;
            GuardianPhone = submission.GuardianPhone;
            Adviser = submission.AdviserSnapshot;
            SchoolName = submission.SchoolSnapshot;
            PrincipalName = submission.PrincipalSnapshot;
            CampusName = submission.CampusSnapshot ?? string.Empty;
            PrincipalTitle = submission.PrincipalTitleSnapshot ?? string.Empty;
            Semester = TemplateConstants.FormatSemester(submission.SemesterSnapshot);
            SchoolYear = submission.SchoolYearSnapshot ?? string.Empty;
            Course = submission.CourseSnapshot ?? string.Empty;

            SelectedSection = Sections.FirstOrDefault(s => s.Id == submission.SectionId);

            if (submission.PhotoPng != null && submission.PhotoPng.Length > 0)
            {
                await SetPhotoBytesAsync(submission.PhotoPng);
            }

            switch (submission.Status)
            {
                case SubmissionStatus.Pending:
                    StatusMessage = "Your ID submission is pending administrator review. Form is locked.";
                    StatusSeverity = InfoBarSeverity.Informational;
                    break;
                case SubmissionStatus.Approved:
                    StatusMessage = "Your ID submission has been approved! Ready to claim. Form is locked.";
                    StatusSeverity = InfoBarSeverity.Success;
                    break;
                case SubmissionStatus.Disapproved:
                    StatusMessage = "Your ID submission was disapproved. Please coordinate with admin or request an ID reset.";
                    StatusSeverity = InfoBarSeverity.Warning;
                    break;
            }
        }
        else
        {
            IsLocked = false;
            StatusMessage = string.Empty;
        }

        IsLoading = false;
    }

    [RelayCommand]
    public void ClearSignature()
    {
        ClearSignatureAction?.Invoke();
        SignatureError = string.Empty;
    }

    [RelayCommand]
    public async Task SubmitAsync()
    {
        if (IsLocked) return;

        ClearErrors();

        byte[]? signatureBytes = null;
        if (SignatureProvider != null)
        {
            signatureBytes = await SignatureProvider.Invoke();
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var birthDateVal = BirthDate ?? DateOnly.MinValue;
        int sectionId = SelectedSection?.Id ?? 0;
        string? cleanBloodType = string.IsNullOrWhiteSpace(BloodType) ? null : BloodType.Trim();

        var validation = InputValidators.ValidateIdSubmission(
            Gender,
            birthDateVal,
            today,
            Address,
            GuardianName,
            GuardianPhone,
            sectionId,
            signatureBytes,
            IsAcceptInfoChecked,
            cleanBloodType);

        if (!validation.IsValid)
        {
            if (validation.Errors.TryGetValue("gender", out var gErr)) GenderError = gErr;
            if (validation.Errors.TryGetValue("birthDate", out var bErr)) BirthDateError = bErr;
            if (validation.Errors.TryGetValue("bloodType", out var btErr)) BloodTypeError = btErr;
            if (validation.Errors.TryGetValue("sectionId", out var sErr)) SectionError = sErr;
            if (validation.Errors.TryGetValue("address", out var aErr)) AddressError = aErr;
            if (validation.Errors.TryGetValue("guardianName", out var gnErr)) GuardianNameError = gnErr;
            if (validation.Errors.TryGetValue("guardianPhone", out var gpErr)) GuardianPhoneError = gpErr;
            if (validation.Errors.TryGetValue("signaturePng", out var sigErr)) SignatureError = sigErr;
            if (validation.Errors.TryGetValue("acceptInfoChecked", out var accErr)) AcceptInfoError = accErr;
            return;
        }

        var user = _userSession.CurrentUser;
        if (user == null) return;

        var submission = new IdSubmission
        {
            UserId = user.Id,
            Gender = Gender,
            BirthDate = birthDateVal,
            BloodType = cleanBloodType,
            Address = Address.Trim(),
            GuardianName = GuardianName.Trim(),
            GuardianPhone = GuardianPhone.Trim(),
            SectionId = sectionId,
            AdviserSnapshot = SelectedSection?.Adviser ?? string.Empty,
            PrincipalSnapshot = PrincipalName,
            SchoolSnapshot = SchoolName,
            SignaturePng = signatureBytes!,
            PhotoPng = PhotoBytes ?? Array.Empty<byte>(),
            Status = SubmissionStatus.Pending,
            SubmittedAt = DateTime.UtcNow
        };

        try
        {
            await _studentService.SubmitIdAsync(submission);
            IsLocked = true;
            StatusMessage = "Your ID information has been submitted successfully and is pending administrator review.";
            StatusSeverity = InfoBarSeverity.Success;
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            StatusSeverity = InfoBarSeverity.Error;
        }
    }

    public async Task SetPhotoBytesAsync(byte[] bytes)
    {
        PhotoError = string.Empty;
        if (bytes == null || bytes.Length == 0)
        {
            RemovePhoto();
            return;
        }

        if (!InputValidators.ValidateImageFile(bytes, 4 * 1024 * 1024, out _, out string? err))
        {
            PhotoError = err ?? "Invalid photo image file. Only PNG and JPG (max 4 MB) are supported.";
            return;
        }

        try
        {
            PhotoBytes = bytes;
            using var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(bytes.AsBuffer());
            stream.Seek(0);
            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream);
            PhotoImage = bitmap;
            HasPhoto = true;
        }
        catch (Exception ex)
        {
            PhotoError = $"Failed to display photo: {ex.Message}";
        }
    }

    public void RemovePhoto()
    {
        PhotoBytes = null;
        PhotoImage = null;
        HasPhoto = false;
        PhotoError = string.Empty;
    }

    private void ClearErrors()
    {
        GenderError = string.Empty;
        BirthDateError = string.Empty;
        BloodTypeError = string.Empty;
        SectionError = string.Empty;
        AddressError = string.Empty;
        GuardianNameError = string.Empty;
        GuardianPhoneError = string.Empty;
        SignatureError = string.Empty;
        PhotoError = string.Empty;
        AcceptInfoError = string.Empty;
    }
}
