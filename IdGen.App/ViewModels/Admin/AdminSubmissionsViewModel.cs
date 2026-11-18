using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IdGen.Core.Enums;
using IdGen.Core.Models;
using IdGen.Core.Services;

namespace IdGen.App.ViewModels.Admin;

public partial class AdminSubmissionsViewModel : ObservableObject
{
    private readonly IAdminService _adminService;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _hasStatusMessage;
    [ObservableProperty] private bool _isStatusError;
    [ObservableProperty] private bool _isEmpty = true;

    public ObservableCollection<IdSubmission> Submissions { get; } = new();

    public AdminSubmissionsViewModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [RelayCommand]
    public async Task LoadSubmissionsAsync()
    {
        IsLoading = true;
        try
        {
            Submissions.Clear();
            var list = await _adminService.GetAllSubmissionsAsync();
            foreach (var sub in list)
            {
                Submissions.Add(sub);
            }
            IsEmpty = Submissions.Count == 0;
        }
        catch (Exception ex)
        {
            ShowStatus($"Failed to load submissions: {ex.Message}", isError: true);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ApproveAsync(IdSubmission submission)
    {
        if (submission == null) return;
        IsLoading = true;
        try
        {
            await _adminService.ApproveSubmissionAsync(submission.Id);
            submission.Status = SubmissionStatus.Approved;
            ShowStatus($"Approved ID submission ID #{submission.Id}.", isError: false);
            await LoadSubmissionsAsync();
        }
        catch (Exception ex)
        {
            ShowStatus($"Failed to approve submission: {ex.Message}", isError: true);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task DisapproveAsync(IdSubmission submission)
    {
        if (submission == null) return;
        IsLoading = true;
        try
        {
            await _adminService.DisapproveSubmissionAsync(submission.Id);
            submission.Status = SubmissionStatus.Disapproved;
            ShowStatus($"Disapproved ID submission ID #{submission.Id}.", isError: false);
            await LoadSubmissionsAsync();
        }
        catch (Exception ex)
        {
            ShowStatus($"Failed to disapprove submission: {ex.Message}", isError: true);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ShowStatus(string message, bool isError)
    {
        StatusMessage = message;
        IsStatusError = isError;
        HasStatusMessage = true;
    }
}
