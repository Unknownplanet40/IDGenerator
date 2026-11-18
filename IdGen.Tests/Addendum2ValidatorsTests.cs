using IdGen.Core.Constants;
using IdGen.Core.Models;
using IdGen.Core.Models.Template;
using IdGen.Core.Validation;
using Xunit;

namespace IdGen.Tests;

public class Addendum2ValidatorsTests
{
    [Theory]
    [InlineData("2024-2025", true)]
    [InlineData("1999-2000", true)]
    [InlineData("2030-2031", true)]
    [InlineData("2024-2026", false)] // difference is 2
    [InlineData("2025-2024", false)] // second < first
    [InlineData("2024-2024", false)] // equal
    [InlineData("2024", false)]      // not range
    [InlineData("abcd-efgh", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidSchoolYear_ValidatesExpected(string? input, bool expectedValid)
    {
        bool isValid = InputValidators.IsValidSchoolYear(input, out string? error);
        Assert.Equal(expectedValid, isValid);
        if (!expectedValid)
        {
            Assert.NotNull(error);
        }
    }

    [Theory]
    [InlineData("First", true)]
    [InlineData("Second", true)]
    [InlineData("Summer", true)]
    [InlineData("first", true)]
    [InlineData("SECOND", true)]
    [InlineData("First Semester", true)]
    [InlineData("second semester", true)]
    [InlineData("SUMMER SEMESTER", true)]
    [InlineData("Third", false)]
    [InlineData("Winter", false)]
    [InlineData("Spring", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidSemester_ValidatesExpected(string? input, bool expectedValid)
    {
        bool isValid = InputValidators.IsValidSemester(input, out string? error);
        Assert.Equal(expectedValid, isValid);
        if (!expectedValid)
        {
            Assert.NotNull(error);
        }
    }

    [Theory]
    [InlineData("First", "First Semester")]
    [InlineData("Second", "Second Semester")]
    [InlineData("Summer", "Summer Semester")]
    [InlineData("First Semester", "First Semester")]
    [InlineData("second semester", "second semester")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void FormatSemester_AppendsSemesterWhenMissing(string? input, string expected)
    {
        string actual = TemplateConstants.FormatSemester(input);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("BSIT", true)]
    [InlineData("Grade 11 - STEM", true)]
    [InlineData("AB-COMM", true)]
    [InlineData("", true)]      // optional
    [InlineData(null, true)]    // optional
    [InlineData("ThisCourseNameIsLongerThanTwentyChars", false)]
    [InlineData("BSIT!@#$", false)]
    public void IsValidCourse_ValidatesExpected(string? input, bool expectedValid)
    {
        bool isValid = InputValidators.IsValidCourse(input, out string? error);
        Assert.Equal(expectedValid, isValid);
        if (!expectedValid)
        {
            Assert.NotNull(error);
        }
    }

    [Fact]
    public void ValidateImageFile_JpegValid_ReturnsTrue()
    {
        byte[] jpegBytes = { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };
        bool isValid = InputValidators.ValidateImageFile(jpegBytes, 2 * 1024 * 1024, out string? mimeType, out string? error);
        Assert.True(isValid);
        Assert.Equal("image/jpeg", mimeType);
        Assert.Null(error);
    }

    [Fact]
    public void ValidateImageFile_PngValid_ReturnsTrue()
    {
        byte[] pngBytes = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00 };
        bool isValid = InputValidators.ValidateImageFile(pngBytes, 2 * 1024 * 1024, out string? mimeType, out string? error);
        Assert.True(isValid);
        Assert.Equal("image/png", mimeType);
        Assert.Null(error);
    }

    [Fact]
    public void ValidateImageFile_InvalidSignature_ReturnsFalse()
    {
        byte[] gifBytes = { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }; // GIF
        bool isValid = InputValidators.ValidateImageFile(gifBytes, 2 * 1024 * 1024, out string? mimeType, out string? error);
        Assert.False(isValid);
        Assert.Null(mimeType);
        Assert.Contains("Only PNG and JPG", error);
    }

    [Fact]
    public void ValidateImageFile_Oversize_ReturnsFalse()
    {
        byte[] pngBytes = new byte[3 * 1024 * 1024]; // 3 MB
        pngBytes[0] = 0x89; pngBytes[1] = 0x50; pngBytes[2] = 0x4E; pngBytes[3] = 0x47;
        pngBytes[4] = 0x0D; pngBytes[5] = 0x0A; pngBytes[6] = 0x1A; pngBytes[7] = 0x0A;

        bool isValid = InputValidators.ValidateImageFile(pngBytes, 2 * 1024 * 1024, out string? mimeType, out string? error);
        Assert.False(isValid);
        Assert.Contains("exceeds 2 MB limit", error);
    }

    [Theory]
    [InlineData("A+", true)]
    [InlineData("A-", true)]
    [InlineData("B+", true)]
    [InlineData("B-", true)]
    [InlineData("AB+", true)]
    [InlineData("AB-", true)]
    [InlineData("O+", true)]
    [InlineData("O-", true)]
    [InlineData("", true)]   // optional
    [InlineData(null, true)] // optional
    [InlineData("C+", false)]
    [InlineData("XYZ", false)]
    public void BloodTypes_ValidatesCorrectly(string? input, bool expectedValid)
    {
        Assert.Equal(expectedValid, BloodTypes.IsValid(input));
    }

    [Fact]
    public void Section_DisplayName_FormatsWithCourse()
    {
        var section = new Section
        {
            Course = "BSIT",
            YearLevel = "3",
            SectionName = "A"
        };
        Assert.Equal("BSIT 3-A", section.DisplayName);
    }

    [Fact]
    public void Section_DisplayName_FormatsWithoutCourse()
    {
        var section = new Section
        {
            Course = null,
            YearLevel = "Grade 11",
            SectionName = "STEM-A"
        };
        Assert.Equal("Grade 11-STEM-A", section.DisplayName);
    }

    [Fact]
    public void ValidateDefaults_ValidInputs_Passes()
    {
        var res = InputValidators.ValidateDefaults(
            "Cavite State University",
            "Main Campus",
            "Dr. Hernando D. Robles",
            "University President",
            "First",
            "2024-2025");

        Assert.True(res.IsValid);
        Assert.Empty(res.Errors);
    }

    [Fact]
    public void ValidateDefaults_InvalidInputs_ReturnsErrors()
    {
        var res = InputValidators.ValidateDefaults(
            "",
            "Main Campus",
            "",
            "",
            "InvalidSem",
            "2024-2026");

        Assert.False(res.IsValid);
        Assert.True(res.HasError("schoolName"));
        Assert.True(res.HasError("principalName"));
        Assert.True(res.HasError("principalTitle"));
        Assert.True(res.HasError("semester"));
        Assert.True(res.HasError("schoolYear"));
    }
}
