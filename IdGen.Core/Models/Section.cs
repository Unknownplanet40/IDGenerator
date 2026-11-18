namespace IdGen.Core.Models;

public class Section
{
    public int Id { get; set; }
    public string? Course { get; set; }
    public string YearLevel { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string Adviser { get; set; } = string.Empty;

    public string DisplayName => !string.IsNullOrWhiteSpace(Course)
        ? $"{Course.Trim()} {YearLevel}-{SectionName}"
        : $"{YearLevel}-{SectionName}";

    public string DisplayText => DisplayName;
}
