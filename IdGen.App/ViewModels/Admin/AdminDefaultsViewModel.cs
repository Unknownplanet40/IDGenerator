using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IdGen.Core.Models;
using IdGen.Core.Services;
using IdGen.Core.Validation;
using IdGen.Data.Repositories;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace IdGen.App.ViewModels.Admin;

public partial class AdminDefaultsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly ITemplateAssetRepository _templateAssetRepository;

    // Original values for dirty tracking
    private string _origSchoolName = string.Empty;
    private string _origCampusName = string.Empty;
    private string _origPrincipalName = string.Empty;
    private string _origPrincipalTitle = string.Empty;
    private string _origSemester = "First";
    private string _origSchoolYear = "2024-2025";
    private int _origLogoId;

    [ObservableProperty] private string _schoolName = string.Empty;
    [ObservableProperty] private string _campusName = string.Empty;
    [ObservableProperty] private string _principalName = string.Empty;
    [ObservableProperty] private string _principalTitle = string.Empty;
    [ObservableProperty] private string _selectedSemester = "First";
    [ObservableProperty] private string _schoolYear = string.Empty;

    // Errors
    [ObservableProperty] private string _schoolNameError = string.Empty;
    [ObservableProperty] private string _campusNameError = string.Empty;
    [ObservableProperty] private string _principalNameError = string.Empty;
    [ObservableProperty] private string _principalTitleError = string.Empty;
    [ObservableProperty] private string _semesterError = string.Empty;
    [ObservableProperty] private string _schoolYearError = string.Empty;
    [ObservableProperty] private string _logoError = string.Empty;

    // Logo
    [ObservableProperty] private BitmapImage? _logoImage;
    [ObservableProperty] private bool _hasLogo;
    [ObservableProperty] private int _logoAssetId;
    private byte[]? _logoBytes;

    // State
    [ObservableProperty] private bool _isDirty;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _hasStatusMessage;
    [ObservableProperty] private bool _isStatusError;
    [ObservableProperty] private bool _isLoading;

    public ObservableCollection<string> SemesterOptions { get; } = new() { "First", "Second", "Summer" };

    public AdminDefaultsViewModel(ISettingsService settingsService, ITemplateAssetRepository templateAssetRepository)
    {
        _settingsService = settingsService;
        _templateAssetRepository = templateAssetRepository;
    }

    partial void OnSchoolNameChanged(string value) => CheckDirty();
    partial void OnCampusNameChanged(string value) => CheckDirty();
    partial void OnPrincipalNameChanged(string value) => CheckDirty();
    partial void OnPrincipalTitleChanged(string value) => CheckDirty();
    partial void OnSelectedSemesterChanged(string value) => CheckDirty();
    partial void OnSchoolYearChanged(string value) => CheckDirty();

    private void CheckDirty()
    {
        IsDirty = SchoolName != _origSchoolName ||
                  CampusName != _origCampusName ||
                  PrincipalName != _origPrincipalName ||
                  PrincipalTitle != _origPrincipalTitle ||
                  SelectedSemester != _origSemester ||
                  SchoolYear != _origSchoolYear ||
                  LogoAssetId != _origLogoId;
    }

    [RelayCommand]
    public async Task LoadDefaultsAsync()
    {
        IsLoading = true;
        ClearErrors();
        try
        {
            var (school, campus, principal, title, semester, schoolYear) = await _settingsService.GetAllSettingsAsync();
            _origSchoolName = school;
            _origCampusName = campus;
            _origPrincipalName = principal;
            _origPrincipalTitle = title;
            _origSemester = string.IsNullOrWhiteSpace(semester) ? "First" : semester;
            _origSchoolYear = schoolYear;

            SchoolName = _origSchoolName;
            CampusName = _origCampusName;
            PrincipalName = _origPrincipalName;
            PrincipalTitle = _origPrincipalTitle;
            SelectedSemester = _origSemester;
            SchoolYear = _origSchoolYear;

            // Load Logo
            var logoAsset = await _templateAssetRepository.GetActiveAssetByKindAsync("SchoolLogo");
            if (logoAsset != null && logoAsset.Data.Length > 0)
            {
                _logoBytes = logoAsset.Data;
                LogoAssetId = logoAsset.Id;
                _origLogoId = logoAsset.Id;
                LogoImage = await CreateBitmapImageAsync(_logoBytes);
                HasLogo = true;
            }
            else
            {
                _logoBytes = null;
                LogoAssetId = 0;
                _origLogoId = 0;
                LogoImage = null;
                HasLogo = false;
            }

            IsDirty = false;
        }
        catch (Exception ex)
        {
            ShowStatus($"Failed to load defaults: {ex.Message}", isError: true);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task UploadLogoAsync()
    {
        ClearErrors();
        try
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");

            if (App.MainWindowInstance != null)
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            }

            var file = await picker.PickSingleFileAsync();
            if (file == null) return;

            using var stream = await file.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.AsStreamForRead().CopyToAsync(ms);
            byte[] fileBytes = ms.ToArray();

            // Validate using InputValidators
            if (!InputValidators.ValidateImageFile(fileBytes, 2 * 1024 * 1024, out string? mimeType, out string? err))
            {
                LogoError = err ?? "Invalid logo image file.";
                return;
            }

            var asset = new TemplateAsset
            {
                Kind = "SchoolLogo",
                MimeType = mimeType!,
                FileSize = fileBytes.Length,
                Data = fileBytes,
                CreatedAt = DateTime.UtcNow
            };

            int id = await _templateAssetRepository.SaveAssetAsync(asset);
            _logoBytes = fileBytes;
            LogoAssetId = id;
            LogoImage = await CreateBitmapImageAsync(fileBytes);
            HasLogo = true;
            LogoError = string.Empty;
            CheckDirty();
            ShowStatus("School logo uploaded successfully.", isError: false);
        }
        catch (Exception ex)
        {
            LogoError = $"Failed to upload logo: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task RemoveLogoAsync()
    {
        ClearErrors();
        try
        {
            await _templateAssetRepository.DeleteByKindAsync("SchoolLogo");
            _logoBytes = null;
            LogoAssetId = 0;
            LogoImage = null;
            HasLogo = false;
            LogoError = string.Empty;
            CheckDirty();
            ShowStatus("School logo removed.", isError: false);
        }
        catch (Exception ex)
        {
            ShowStatus($"Failed to remove logo: {ex.Message}", isError: true);
        }
    }

    [RelayCommand]
    public async Task SaveDefaultsAsync()
    {
        ClearErrors();

        var valResult = InputValidators.ValidateDefaults(
            SchoolName,
            CampusName,
            PrincipalName,
            PrincipalTitle,
            SelectedSemester,
            SchoolYear);

        if (!valResult.IsValid)
        {
            SchoolNameError = valResult.Errors.GetValueOrDefault(nameof(SchoolName), "");
            CampusNameError = valResult.Errors.GetValueOrDefault(nameof(CampusName), "");
            PrincipalNameError = valResult.Errors.GetValueOrDefault(nameof(PrincipalName), "");
            PrincipalTitleError = valResult.Errors.GetValueOrDefault(nameof(PrincipalTitle), "");
            SemesterError = valResult.Errors.GetValueOrDefault(nameof(SelectedSemester), "");
            SchoolYearError = valResult.Errors.GetValueOrDefault(nameof(SchoolYear), "");

            ShowStatus("Please fix the validation errors before saving.", isError: true);
            return;
        }

        IsLoading = true;
        try
        {
            await _settingsService.SaveAllSettingsAsync(
                SchoolName,
                CampusName,
                PrincipalName,
                PrincipalTitle,
                SelectedSemester,
                SchoolYear);

            _origSchoolName = SchoolName;
            _origCampusName = CampusName;
            _origPrincipalName = PrincipalName;
            _origPrincipalTitle = PrincipalTitle;
            _origSemester = SelectedSemester;
            _origSchoolYear = SchoolYear;
            _origLogoId = LogoAssetId;

            IsDirty = false;
            ShowStatus("Institutional settings saved successfully.", isError: false);
        }
        catch (Exception ex)
        {
            ShowStatus($"Failed to save defaults: {ex.Message}", isError: true);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ClearErrors()
    {
        SchoolNameError = string.Empty;
        CampusNameError = string.Empty;
        PrincipalNameError = string.Empty;
        PrincipalTitleError = string.Empty;
        SemesterError = string.Empty;
        SchoolYearError = string.Empty;
        LogoError = string.Empty;
        HasStatusMessage = false;
    }

    private void ShowStatus(string message, bool isError)
    {
        StatusMessage = message;
        IsStatusError = isError;
        HasStatusMessage = true;
    }

    private static async Task<BitmapImage?> CreateBitmapImageAsync(byte[]? bytes)
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
