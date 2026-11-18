using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IdGen.Core.Models;
using IdGen.Core.Services;

namespace IdGen.App.ViewModels.Admin;

public partial class AdminRegistrationsViewModel : ObservableObject
{
    private readonly IAdminService _adminService;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _hasStatusMessage;

    [ObservableProperty]
    private bool _isStatusError;

    [ObservableProperty]
    private bool _isEmpty = true;

    public ObservableCollection<User> PendingRegistrations { get; } = new();

    public AdminRegistrationsViewModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [RelayCommand]
    public async Task LoadRegistrationsAsync()
    {
        IsLoading = true;
        try
        {
            PendingRegistrations.Clear();
            var list = await _adminService.GetPendingRegistrationsAsync();
            foreach (var user in list)
            {
                PendingRegistrations.Add(user);
            }
            IsEmpty = PendingRegistrations.Count == 0;
        }
        catch (Exception ex)
        {
            ShowStatus($"Failed to load registrations: {ex.Message}", isError: true);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ApproveAsync(User user)
    {
        if (user == null) return;
        IsLoading = true;
        try
        {
            await _adminService.ApproveRegistrationAsync(user.Id);
            PendingRegistrations.Remove(user);
            IsEmpty = PendingRegistrations.Count == 0;
            ShowStatus($"Approved registration for {user.FullName} ({user.StudentNo}).", isError: false);
        }
        catch (Exception ex)
        {
            ShowStatus($"Failed to approve registration: {ex.Message}", isError: true);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task DeclineAsync(User user)
    {
        if (user == null) return;
        IsLoading = true;
        try
        {
            await _adminService.DeclineRegistrationAsync(user.Id);
            PendingRegistrations.Remove(user);
            IsEmpty = PendingRegistrations.Count == 0;
            ShowStatus($"Declined registration for {user.FullName}.", isError: false);
        }
        catch (Exception ex)
        {
            ShowStatus($"Failed to decline registration: {ex.Message}", isError: true);
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
