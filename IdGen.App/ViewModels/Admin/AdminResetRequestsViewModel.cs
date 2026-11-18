using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IdGen.Core.Enums;
using IdGen.Core.Models;
using IdGen.Core.Services;

namespace IdGen.App.ViewModels.Admin;

public partial class AdminResetRequestsViewModel : ObservableObject
{
    private readonly IAdminService _adminService;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _hasStatusMessage;
    [ObservableProperty] private bool _isStatusError;
    [ObservableProperty] private bool _isEmpty = true;
    [ObservableProperty] private bool _showAllRequests; // false = Pending only (default), true = All history

    public ObservableCollection<ResetRequest> ResetRequests { get; } = new();

    public AdminResetRequestsViewModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [RelayCommand]
    public async Task SetFilterAsync(string filter)
    {
        ShowAllRequests = (filter == "All");
        await LoadResetRequestsAsync();
    }

    [RelayCommand]
    public async Task LoadResetRequestsAsync()
    {
        IsLoading = true;
        try
        {
            ResetRequests.Clear();
            var list = ShowAllRequests
                ? await _adminService.GetAllResetRequestsAsync()
                : await _adminService.GetPendingResetRequestsAsync();

            foreach (var req in list)
            {
                ResetRequests.Add(req);
            }
            IsEmpty = ResetRequests.Count == 0;
        }
        catch (Exception ex)
        {
            ShowStatus($"Failed to load reset requests: {ex.Message}", isError: true);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ApproveAsync(ResetRequest request)
    {
        if (request == null) return;
        IsLoading = true;
        try
        {
            await _adminService.ApproveResetRequestAsync(request.Id);
            request.Status = ResetStatus.Approved;
            request.ResolvedAt = DateTime.UtcNow;
            ShowStatus($"Approved reset request #{request.Id}. Student's submission has been unlocked.", isError: false);
            await LoadResetRequestsAsync();
        }
        catch (Exception ex)
        {
            ShowStatus($"Failed to approve reset request: {ex.Message}", isError: true);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task DeclineAsync(ResetRequest request)
    {
        if (request == null) return;
        IsLoading = true;
        try
        {
            await _adminService.DeclineResetRequestAsync(request.Id);
            request.Status = ResetStatus.Declined;
            request.ResolvedAt = DateTime.UtcNow;
            ShowStatus($"Declined reset request #{request.Id}.", isError: false);
            await LoadResetRequestsAsync();
        }
        catch (Exception ex)
        {
            ShowStatus($"Failed to decline reset request: {ex.Message}", isError: true);
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
