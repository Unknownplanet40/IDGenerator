using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IdGen.App.Services;
using IdGen.Core.Enums;
using IdGen.Core.Services;
using Microsoft.UI.Xaml.Controls;

namespace IdGen.App.ViewModels.Student;

public partial class StudentResetIdViewModel : ObservableObject
{
    private readonly IStudentService _studentService;
    private readonly IUserSession _userSession;

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private bool _hasSubmission;

    [ObservableProperty]
    private bool _hasPendingRequest;

    [ObservableProperty]
    private bool _canRequestReset;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private InfoBarSeverity _statusSeverity = InfoBarSeverity.Informational;

    [ObservableProperty]
    private string _requestDetails = string.Empty;

    public StudentResetIdViewModel(IStudentService studentService, IUserSession userSession)
    {
        _studentService = studentService;
        _userSession = userSession;
    }

    [RelayCommand]
    public async Task LoadStateAsync()
    {
        IsLoading = true;
        StatusMessage = string.Empty;

        var user = _userSession.CurrentUser;
        if (user == null)
        {
            IsLoading = false;
            return;
        }

        var submission = await _studentService.GetSubmissionAsync(user.Id);
        HasSubmission = submission != null;

        var pendingReq = await _studentService.GetPendingResetRequestAsync(user.Id);
        HasPendingRequest = pendingReq != null;

        if (!HasSubmission)
        {
            CanRequestReset = false;
            RequestDetails = "You currently do not have an active ID submission. Reset is only available for students with an existing submission.";
            StatusMessage = "No active ID submission to reset.";
            StatusSeverity = InfoBarSeverity.Informational;
        }
        else if (HasPendingRequest)
        {
            CanRequestReset = false;
            RequestDetails = $"You have a pending ID reset request submitted on {pendingReq!.RequestedAtDisplay}. Please wait for an administrator to review your request.";
            StatusMessage = "Reset request pending administrator review.";
            StatusSeverity = InfoBarSeverity.Warning;
        }
        else
        {
            CanRequestReset = true;
            RequestDetails = "Submitting an ID reset request allows you to ask administrators to unlock your ID Generator so you can update your information. Once approved, your previous submission will be cleared and the form will be enabled.";
            StatusMessage = "Eligible to request ID reset.";
            StatusSeverity = InfoBarSeverity.Informational;
        }

        IsLoading = false;
    }

    [RelayCommand]
    public async Task RequestResetAsync()
    {
        if (!CanRequestReset) return;

        var user = _userSession.CurrentUser;
        if (user == null) return;

        try
        {
            var req = await _studentService.RequestResetAsync(user.Id);
            HasPendingRequest = true;
            CanRequestReset = false;
            RequestDetails = $"Your ID reset request has been submitted on {req.RequestedAtDisplay}. Waiting for administrator approval.";
            StatusMessage = "Reset request submitted successfully!";
            StatusSeverity = InfoBarSeverity.Success;
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            StatusSeverity = InfoBarSeverity.Error;
        }
    }
}
