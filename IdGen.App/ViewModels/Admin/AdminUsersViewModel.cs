using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IdGen.Core.Enums;
using IdGen.Core.Models;
using IdGen.Core.Services;

namespace IdGen.App.ViewModels.Admin;

public partial class AdminUsersViewModel : ObservableObject
{
    private readonly IAdminService _adminService;
    private readonly List<User> _allUsers = new();

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isEmpty = true;
    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private string _roleFilter = "All"; // All, Admin, Student
    [ObservableProperty] private string _statusFilter = "All"; // All, Approved, Pending, Declined
    [ObservableProperty] private string _summaryText = "Loading users...";
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _hasStatusMessage;
    [ObservableProperty] private bool _isStatusError;

    public ObservableCollection<User> FilteredUsers { get; } = new();

    public AdminUsersViewModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [RelayCommand]
    public async Task LoadUsersAsync()
    {
        IsLoading = true;
        try
        {
            var list = await _adminService.GetAllUsersAsync();
            _allUsers.Clear();
            _allUsers.AddRange(list);
            ApplyFilters();
        }
        catch (Exception ex)
        {
            ShowStatus($"Failed to load users: {ex.Message}", isError: true);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void SetRoleFilter(string role)
    {
        RoleFilter = role;
        ApplyFilters();
    }

    [RelayCommand]
    public void SetStatusFilter(string status)
    {
        StatusFilter = status;
        ApplyFilters();
    }

    [RelayCommand]
    public void Search(string query)
    {
        SearchQuery = query;
        ApplyFilters();
    }

    public void ApplyFilters()
    {
        FilteredUsers.Clear();

        var query = _allUsers.AsEnumerable();

        // 1. Role Filter
        if (!string.Equals(RoleFilter, "All", StringComparison.OrdinalIgnoreCase))
        {
            if (Enum.TryParse<UserRole>(RoleFilter, out var role))
            {
                query = query.Where(u => u.Role == role);
            }
        }

        // 2. Status Filter
        if (!string.Equals(StatusFilter, "All", StringComparison.OrdinalIgnoreCase))
        {
            if (Enum.TryParse<RegistrationStatus>(StatusFilter, out var status))
            {
                query = query.Where(u => u.Status == status);
            }
        }

        // 3. Search Query
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            string term = SearchQuery.Trim().ToLowerInvariant();
            query = query.Where(u =>
                (u.FullName?.ToLowerInvariant().Contains(term) ?? false) ||
                (u.Username?.ToLowerInvariant().Contains(term) ?? false) ||
                (u.StudentNo?.ToLowerInvariant().Contains(term) ?? false) ||
                (u.Email?.ToLowerInvariant().Contains(term) ?? false) ||
                (u.Phone?.ToLowerInvariant().Contains(term) ?? false));
        }

        foreach (var u in query)
        {
            FilteredUsers.Add(u);
        }

        IsEmpty = FilteredUsers.Count == 0;
        SummaryText = $"Showing {FilteredUsers.Count} of {_allUsers.Count} user accounts";
    }

    private void ShowStatus(string message, bool isError)
    {
        StatusMessage = message;
        IsStatusError = isError;
        HasStatusMessage = true;
    }
}
