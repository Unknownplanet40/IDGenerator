namespace IdGen.Core.Models.Template;

public static class TemplateConstants
{
    public const int MaxElementsPerSide = 80;

    // CR80 standard credit card dimensions at 300 DPI: 53.98mm x 85.60mm
    public const double StandardPortraitWidth = 638.0;
    public const double StandardPortraitHeight = 1011.0;

    public const double StandardLandscapeWidth = 1011.0;
    public const double StandardLandscapeHeight = 638.0;

    public static readonly string[] WhitelistedFonts =
    [
        "Segoe UI",
        "Arial",
        "Times New Roman",
        "Georgia",
        "Calibri",
        "Cambria",
        "Verdana",
        "Tahoma",
        "Trebuchet MS",
        "Impact"
    ];

    public static readonly string[] WhitelistedDynamicFields =
    [
        "student_no",
        "first_name",
        "last_name",
        "middle_name",
        "full_name",
        "course",
        "year_level",
        "section_name",
        "adviser",
        "gender",
        "birth_date",
        "age",
        "blood_type",
        "address",
        "guardian_name",
        "guardian_phone",
        "school_name",
        "campus_name",
        "principal_name",
        "principal_title",
        "semester",
        "school_year"
    ];

    public static readonly string[] ElementTypes =
    [
        "Rectangle",
        "Line",
        "Barcode",
        "StaticText",
        "DynamicText",
        "Logo",
        "Photo",
        "Signature"
    ];

    public static readonly string[] BackgroundFitModes =
    [
        "Cover",
        "Stretch",
        "Contain"
    ];

    public static string FormatSemester(string? semester)
    {
        if (string.IsNullOrWhiteSpace(semester))
            return string.Empty;

        string trimmed = semester.Trim();
        if (trimmed.EndsWith("Semester", StringComparison.OrdinalIgnoreCase))
            return trimmed;

        return $"{trimmed} Semester";
    }
}
