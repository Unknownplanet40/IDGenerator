using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IdGen.Core.Exceptions;
using IdGen.Core.Models;
using IdGen.Core.Services;
using IdGen.Core.Validation;

namespace IdGen.App.ViewModels.Admin;

public partial class AdminSectionsViewModel : ObservableObject
{
    private readonly IAdminService _adminService;

    // Form Fields
    [ObservableProperty] private string _course = string.Empty;
    [ObservableProperty] private string _yearLevel = string.Empty;
    [ObservableProperty] private string _sectionName = string.Empty;
    [ObservableProperty] private string _adviser = string.Empty;

    // Form Errors
    [ObservableProperty] private string _courseError = string.Empty;
    [ObservableProperty] private string _yearLevelError = string.Empty;
    [ObservableProperty] private string _sectionNameError = string.Empty;
    [ObservableProperty] private string _adviserError = string.Empty;

    // Edit Mode State
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private int? _editingSectionId;
    [ObservableProperty] private string _formTitle = "Add New Section";
    [ObservableProperty] private string _submitButtonText = "Add Section";

    // Search and Filtering
    [ObservableProperty] private string _searchQuery = string.Empty;

    // Status and Loading
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _hasStatusMessage;
    [ObservableProperty] private bool _isStatusError;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isEmpty = true;

    public ObservableCollection<Section> Sections { get; } = new();

    public AdminSectionsViewModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    partial void OnSearchQueryChanged(string value)
    {
        _ = LoadSectionsAsync();
    }

    [RelayCommand]
    public async Task LoadSectionsAsync()
    {
        IsLoading = true;
        try
        {
            Sections.Clear();
            var list = string.IsNullOrWhiteSpace(SearchQuery)
                ? await _adminService.GetAllSectionsAsync()
                : await _adminService.SearchSectionsAsync(SearchQuery);

            // Sort by Course, YearLevel, then SectionName
            var sorted = list.OrderBy(s => s.Course ?? string.Empty)
                             .ThenBy(s => s.YearLevel)
                             .ThenBy(s => s.SectionName);

            foreach (var s in sorted)
            {
                Sections.Add(s);
            }
            IsEmpty = Sections.Count == 0;
        }
        catch (Exception ex)
        {
            ShowStatus($"Failed to load sections: {ex.Message}", isError: true);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void BeginEdit(Section? section)
    {
        if (section == null) return;

        ClearValidationErrors();
        EditingSectionId = section.Id;
        Course = section.Course ?? string.Empty;
        YearLevel = section.YearLevel;
        SectionName = section.SectionName;
        Adviser = section.Adviser;

        IsEditing = true;
        FormTitle = $"Edit Section: {section.DisplayName}";
        SubmitButtonText = "Save Changes";
    }

    [RelayCommand]
    public void CancelEdit()
    {
        ClearValidationErrors();
        EditingSectionId = null;
        Course = string.Empty;
        YearLevel = string.Empty;
        SectionName = string.Empty;
        Adviser = string.Empty;

        IsEditing = false;
        FormTitle = "Add New Section";
        SubmitButtonText = "Add Section";
    }

    [RelayCommand]
    public async Task SaveSectionAsync()
    {
        ClearValidationErrors();

        var validation = InputValidators.ValidateSection(YearLevel, SectionName, Adviser, Course);
        if (!validation.IsValid)
        {
            CourseError = validation.Errors.GetValueOrDefault(nameof(Course), "");
            YearLevelError = validation.Errors.GetValueOrDefault(nameof(YearLevel), "");
            SectionNameError = validation.Errors.GetValueOrDefault(nameof(SectionName), "");
            AdviserError = validation.Errors.GetValueOrDefault(nameof(Adviser), "");

            ShowStatus("Please fix the validation errors before saving.", isError: true);
            return;
        }

        IsLoading = true;
        try
        {
            if (IsEditing && EditingSectionId.HasValue)
            {
                var updated = await _adminService.UpdateSectionAsync(
                    EditingSectionId.Value,
                    YearLevel,
                    SectionName,
                    Adviser,
                    string.IsNullOrWhiteSpace(Course) ? null : Course.Trim());

                ShowStatus($"Updated section '{updated.DisplayName}' successfully.", isError: false);
                CancelEdit();
            }
            else
            {
                var created = await _adminService.AddSectionAsync(
                    YearLevel,
                    SectionName,
                    Adviser,
                    string.IsNullOrWhiteSpace(Course) ? null : Course.Trim());

                ShowStatus($"Added section '{created.DisplayName}' successfully.", isError: false);
                CancelEdit();
            }

            await LoadSectionsAsync();
        }
        catch (InvalidOperationException ex)
        {
            ShowStatus(ex.Message, isError: true);
        }
        catch (Exception ex)
        {
            ShowStatus(ex.Message, isError: true);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task DeleteSectionAsync(Section? section)
    {
        if (section == null) return;

        IsLoading = true;
        try
        {
            await _adminService.DeleteSectionAsync(section.Id);
            Sections.Remove(section);
            IsEmpty = Sections.Count == 0;
            ShowStatus($"Section '{section.DisplayName}' deleted successfully.", isError: false);

            if (IsEditing && EditingSectionId == section.Id)
            {
                CancelEdit();
            }
        }
        catch (SectionInUseException)
        {
            ShowStatus("Cannot delete this section because it is currently assigned to one or more ID submissions.", isError: true);
        }
        catch (Exception ex)
        {
            ShowStatus($"Failed to delete section: {ex.Message}", isError: true);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ClearValidationErrors()
    {
        CourseError = string.Empty;
        YearLevelError = string.Empty;
        SectionNameError = string.Empty;
        AdviserError = string.Empty;
        HasStatusMessage = false;
    }

    private void ShowStatus(string message, bool isError)
    {
        StatusMessage = message;
        IsStatusError = isError;
        HasStatusMessage = true;
    }
}
