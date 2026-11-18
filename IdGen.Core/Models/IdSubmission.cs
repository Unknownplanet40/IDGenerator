using IdGen.Core.Enums;
using IdGen.Core.Services;

namespace IdGen.Core.Models;

public class IdSubmission
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Gender { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string? BloodType { get; set; }
    public string Address { get; set; } = string.Empty;
    public string GuardianName { get; set; } = string.Empty;
    public string GuardianPhone { get; set; } = string.Empty;
    public int SectionId { get; set; }

    // Snapshot columns (immutable after submission)
    public string AdviserSnapshot { get; set; } = string.Empty;
    public string PrincipalSnapshot { get; set; } = string.Empty;
    public string SchoolSnapshot { get; set; } = string.Empty;
    public string? CourseSnapshot { get; set; }
    public string? CampusSnapshot { get; set; }
    public string? PrincipalTitleSnapshot { get; set; }
    public string? SemesterSnapshot { get; set; }
    public string? SchoolYearSnapshot { get; set; }

    public byte[] SignaturePng { get; set; } = Array.Empty<byte>();
    public byte[] PhotoPng { get; set; } = Array.Empty<byte>();
    public SubmissionStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // Joined student profile info (for display/review)
    public string? StudentNo { get; set; }
    public string? StudentName { get; set; }

    public string StudentNoDisplay => string.IsNullOrWhiteSpace(StudentNo) ? $"#{UserId}" : StudentNo;
    public string StudentNameDisplay => string.IsNullOrWhiteSpace(StudentName) ? "Student" : StudentName;
    public bool IsPending => Status == SubmissionStatus.Pending;
    public bool IsApproved => Status == SubmissionStatus.Approved;

    public string SubmittedAtDisplay => SubmittedAt.ToString("yyyy-MM-dd HH:mm");
    public string BirthDateDisplay => BirthDate.ToString("yyyy-MM-dd");

    public int ComputeAge(DateOnly? asOfDate = null)
    {
        DateOnly referenceDate = asOfDate ?? DateOnly.FromDateTime(DateTime.Today);
        return AgeCalculator.Calculate(BirthDate, referenceDate);
    }
}
