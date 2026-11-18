using System.Net.Mail;
using System.Text.RegularExpressions;
using IdGen.Core.Constants;
using IdGen.Core.Enums;
using IdGen.Core.Models;

namespace IdGen.Core.Validation;

public static class InputValidators
{
    private static readonly Regex PhPhoneRegex = new(@"^(09|\+639)\d{9}$", RegexOptions.Compiled);
    private static readonly Regex StudentNoRegex = new(@"^\d{9}$", RegexOptions.Compiled);
    private static readonly Regex SchoolYearRegex = new(@"^(\d{4})-(\d{4})$", RegexOptions.Compiled);
    private static readonly Regex CourseRegex = new(@"^[a-zA-Z0-9\s\-]+$", RegexOptions.Compiled);

    private static readonly byte[] JpegHeader = { 0xFF, 0xD8, 0xFF };
    private static readonly byte[] PngHeader = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

    public static bool IsValidStudentNo(string? studentNo)
    {
        if (string.IsNullOrWhiteSpace(studentNo)) return false;
        string cleaned = studentNo.Trim().Replace("-", "");
        return StudentNoRegex.IsMatch(cleaned);
    }

    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        if (email.Length > 100) return false;
        return MailAddress.TryCreate(email, out _);
    }

    public static bool IsValidPhPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return false;
        string cleaned = phone.Trim().Replace(" ", "").Replace("-", "");
        return PhPhoneRegex.IsMatch(cleaned);
    }

    public static bool IsValidSchoolYear(string? schoolYear, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(schoolYear))
        {
            error = "School year is required.";
            return false;
        }

        var match = SchoolYearRegex.Match(schoolYear.Trim());
        if (!match.Success)
        {
            error = "School year must be in format YYYY-YYYY (e.g. 2024-2025).";
            return false;
        }

        if (int.TryParse(match.Groups[1].Value, out int y1) && int.TryParse(match.Groups[2].Value, out int y2))
        {
            if (y2 != y1 + 1)
            {
                error = $"Second year must equal first year + 1 (e.g. {y1}-{y1 + 1}).";
                return false;
            }
            return true;
        }

        error = "Invalid year numbers in school year.";
        return false;
    }

    public static bool IsValidSemester(string? semester, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(semester))
        {
            error = "Semester is required.";
            return false;
        }

        string clean = semester.Trim();
        if (clean.EndsWith("Semester", StringComparison.OrdinalIgnoreCase))
        {
            clean = clean[..^"Semester".Length].Trim();
        }

        if (Enum.TryParse<Semester>(clean, true, out _))
        {
            return true;
        }

        error = "Semester must be First, Second, or Summer.";
        return false;
    }

    public static bool IsValidCourse(string? course, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(course))
        {
            return true; // Optional on sections
        }

        string trimmed = course.Trim();
        if (trimmed.Length > 20)
        {
            error = "Course cannot exceed 20 characters.";
            return false;
        }

        if (!CourseRegex.IsMatch(trimmed))
        {
            error = "Course may only contain letters, numbers, spaces, and hyphens.";
            return false;
        }

        return true;
    }

    public static bool ValidateImageFile(byte[]? data, int maxBytes, out string? mimeType, out string? error)
    {
        mimeType = null;
        error = null;

        if (data == null || data.Length == 0)
        {
            error = "File data cannot be empty.";
            return false;
        }

        if (data.Length > maxBytes)
        {
            int maxMb = maxBytes / (1024 * 1024);
            error = $"File size exceeds {maxMb} MB limit.";
            return false;
        }

        // Check JPEG signature
        if (data.Length >= JpegHeader.Length &&
            data[0] == JpegHeader[0] &&
            data[1] == JpegHeader[1] &&
            data[2] == JpegHeader[2])
        {
            mimeType = "image/jpeg";
            return true;
        }

        // Check PNG signature
        if (data.Length >= PngHeader.Length)
        {
            bool isPng = true;
            for (int i = 0; i < PngHeader.Length; i++)
            {
                if (data[i] != PngHeader[i])
                {
                    isPng = false;
                    break;
                }
            }
            if (isPng)
            {
                mimeType = "image/png";
                return true;
            }
        }

        error = "Invalid image file format. Only PNG and JPG images are supported.";
        return false;
    }

    public static ValidationResult ValidateRegistration(
        string? studentNo,
        string? lastName,
        string? firstName,
        string? middleName,
        string? email,
        string? phone,
        string? username,
        string? password,
        string? confirmPassword)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(studentNo))
            result.AddError(nameof(studentNo), "Student ID number is required.");
        else if (!IsValidStudentNo(studentNo))
            result.AddError(nameof(studentNo), "Student ID number must be 9 digits (e.g. 202110055).");

        if (string.IsNullOrWhiteSpace(lastName))
            result.AddError(nameof(lastName), "Last name is required.");
        else if (lastName.Trim().Length > 50)
            result.AddError(nameof(lastName), "Last name cannot exceed 50 characters.");

        if (string.IsNullOrWhiteSpace(firstName))
            result.AddError(nameof(firstName), "First name is required.");
        else if (firstName.Trim().Length > 50)
            result.AddError(nameof(firstName), "First name cannot exceed 50 characters.");

        if (!string.IsNullOrWhiteSpace(middleName) && middleName.Trim().Length > 50)
            result.AddError(nameof(middleName), "Middle name cannot exceed 50 characters.");

        if (string.IsNullOrWhiteSpace(email))
            result.AddError(nameof(email), "Email is required.");
        else if (!IsValidEmail(email.Trim()))
            result.AddError(nameof(email), "Invalid email format.");

        if (string.IsNullOrWhiteSpace(phone))
            result.AddError(nameof(phone), "Phone number is required.");
        else if (!IsValidPhPhone(phone.Trim()))
            result.AddError(nameof(phone), "Must be a valid PH phone number (09XXXXXXXXX or +639XXXXXXXXX).");

        if (string.IsNullOrWhiteSpace(username))
            result.AddError(nameof(username), "Username is required.");
        else if (username.Trim().Length < 3 || username.Trim().Length > 50)
            result.AddError(nameof(username), "Username must be between 3 and 50 characters.");

        if (string.IsNullOrWhiteSpace(password))
            result.AddError(nameof(password), "Password is required.");
        else if (password.Length < 8)
            result.AddError(nameof(password), "Password must be at least 8 characters.");

        if (string.IsNullOrWhiteSpace(confirmPassword))
            result.AddError(nameof(confirmPassword), "Please confirm your password.");
        else if (password != confirmPassword)
            result.AddError(nameof(confirmPassword), "Passwords do not match.");

        return result;
    }

    public static ValidationResult ValidateAdminSetup(string? username, string? password, string? confirmPassword)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(username))
            result.AddError(nameof(username), "Admin username is required.");
        else if (username.Trim().Length < 3 || username.Trim().Length > 50)
            result.AddError(nameof(username), "Username must be between 3 and 50 characters.");

        if (string.IsNullOrWhiteSpace(password))
            result.AddError(nameof(password), "Password is required.");
        else if (password.Length < 8)
            result.AddError(nameof(password), "Password must be at least 8 characters.");

        if (string.IsNullOrWhiteSpace(confirmPassword))
            result.AddError(nameof(confirmPassword), "Please confirm your password.");
        else if (password != confirmPassword)
            result.AddError(nameof(confirmPassword), "Passwords do not match.");

        return result;
    }

    public static ValidationResult ValidateSection(string? yearLevel, string? sectionName, string? adviser, string? course = null)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(yearLevel))
            result.AddError(nameof(yearLevel), "Year level is required.");
        else if (yearLevel.Trim().Length > 20)
            result.AddError(nameof(yearLevel), "Year level cannot exceed 20 characters.");

        if (string.IsNullOrWhiteSpace(sectionName))
            result.AddError(nameof(sectionName), "Section name is required.");
        else if (sectionName.Trim().Length > 50)
            result.AddError(nameof(sectionName), "Section name cannot exceed 50 characters.");

        if (string.IsNullOrWhiteSpace(adviser))
            result.AddError(nameof(adviser), "Adviser is required.");
        else if (adviser.Trim().Length > 100)
            result.AddError(nameof(adviser), "Adviser name cannot exceed 100 characters.");

        if (!IsValidCourse(course, out string? courseErr))
            result.AddError(nameof(course), courseErr!);

        return result;
    }

    public static ValidationResult ValidateDefaults(
        string? schoolName,
        string? campusName,
        string? principalName,
        string? principalTitle,
        string? semester,
        string? schoolYear)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(schoolName))
            result.AddError(nameof(schoolName), "School name is required.");
        else if (schoolName.Trim().Length > 100)
            result.AddError(nameof(schoolName), "School name cannot exceed 100 characters.");

        if (!string.IsNullOrWhiteSpace(campusName) && campusName.Trim().Length > 100)
            result.AddError(nameof(campusName), "Campus name cannot exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(principalName))
            result.AddError(nameof(principalName), "Signatory name is required.");
        else if (principalName.Trim().Length > 100)
            result.AddError(nameof(principalName), "Signatory name cannot exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(principalTitle))
            result.AddError(nameof(principalTitle), "Signatory title is required.");
        else if (principalTitle.Trim().Length > 100)
            result.AddError(nameof(principalTitle), "Signatory title cannot exceed 100 characters.");

        if (!IsValidSemester(semester, out string? semErr))
            result.AddError(nameof(semester), semErr!);

        if (!IsValidSchoolYear(schoolYear, out string? syErr))
            result.AddError(nameof(schoolYear), syErr!);

        return result;
    }

    public static ValidationResult ValidateIdSubmission(
        string? gender,
        DateOnly birthDate,
        DateOnly today,
        string? address,
        string? guardianName,
        string? guardianPhone,
        int sectionId,
        byte[]? signaturePng,
        bool acceptInfoChecked,
        string? bloodType = null)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(gender) || (gender != "Male" && gender != "Female"))
            result.AddError(nameof(gender), "Please select a valid gender (Male or Female).");

        if (birthDate > today)
            result.AddError(nameof(birthDate), "Birth date cannot be in the future.");

        if (!string.IsNullOrWhiteSpace(bloodType) && !BloodTypes.IsValid(bloodType))
            result.AddError(nameof(bloodType), "Invalid blood type selected.");

        if (string.IsNullOrWhiteSpace(address))
            result.AddError(nameof(address), "Complete address is required.");
        else if (address.Trim().Length > 250)
            result.AddError(nameof(address), "Address cannot exceed 250 characters.");

        if (string.IsNullOrWhiteSpace(guardianName))
            result.AddError(nameof(guardianName), "Guardian name is required.");
        else if (guardianName.Trim().Length > 100)
            result.AddError(nameof(guardianName), "Guardian name cannot exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(guardianPhone))
            result.AddError(nameof(guardianPhone), "Guardian phone number is required.");
        else if (!IsValidPhPhone(guardianPhone.Trim()))
            result.AddError(nameof(guardianPhone), "Must be a valid PH phone number (09XXXXXXXXX or +639XXXXXXXXX).");

        if (sectionId <= 0)
            result.AddError(nameof(sectionId), "Please select a valid Year & Section.");

        if (signaturePng == null || signaturePng.Length == 0)
            result.AddError(nameof(signaturePng), "Signature is required.");

        if (!acceptInfoChecked)
            result.AddError(nameof(acceptInfoChecked), "You must accept all information as correct to submit.");

        return result;
    }

    public static ValidationResult ValidatePasswordChange(string? currentPassword, string? newPassword, string? confirmPassword)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(currentPassword))
            result.AddError(nameof(currentPassword), "Current password is required.");

        if (string.IsNullOrWhiteSpace(newPassword))
            result.AddError(nameof(newPassword), "New password is required.");
        else if (newPassword.Length < 8)
            result.AddError(nameof(newPassword), "New password must be at least 8 characters.");

        if (string.IsNullOrWhiteSpace(confirmPassword))
            result.AddError(nameof(confirmPassword), "Please confirm your new password.");
        else if (newPassword != confirmPassword)
            result.AddError(nameof(confirmPassword), "Passwords do not match.");

        return result;
    }
}
