using IdGen.Core.Enums;

namespace IdGen.Core.Models;

public class ResetRequest
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public ResetStatus Status { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    public string? StudentNo { get; set; }
    public string? StudentName { get; set; }

    public string StudentNoDisplay => string.IsNullOrWhiteSpace(StudentNo) ? $"#{UserId}" : StudentNo;
    public string StudentNameDisplay => string.IsNullOrWhiteSpace(StudentName) ? "Student" : StudentName;
    public bool IsPending => Status == ResetStatus.Pending;

    public string RequestedAtDisplay => RequestedAt.ToString("yyyy-MM-dd HH:mm");
    public string ResolvedAtDisplay => ResolvedAt.HasValue ? ResolvedAt.Value.ToString("yyyy-MM-dd HH:mm") : "-";
}
